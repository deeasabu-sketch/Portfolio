using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioApp.Data;
using PortfolioApp.Models;
using PortfolioApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be set in appsettings.json.");

builder.Services.AddDbContext<PortfolioDbContext>(options =>
    options.UseSqlServer(connectionString));

// ---------- JWT Auth ----------
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be set in appsettings.json or user secrets before running.");
builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"] ?? "PortfolioApp",
        ValidAudience = jwtSection["Audience"] ?? "PortfolioApp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ---------- CORS ----------
// Allows the static frontend (index.html / admin.html) to call the API
// even if it's opened from a different origin during development.
builder.Services.AddCors(options =>
{
    options.AddPolicy("PortfolioFrontend", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

// ---------- DB migration + first-run admin seeding ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    db.Database.Migrate();

    if (!db.AdminUsers.Any())
    {
        // First run only: create the single admin row.
        // Reads the initial password from configuration (appsettings / user secrets / env var),
        // never hardcoded in source, and is hashed immediately — the plain password is not stored.
        var initialPassword = app.Configuration["AdminSetup:InitialPassword"];
        if (string.IsNullOrWhiteSpace(initialPassword))
        {
            throw new InvalidOperationException(
                "No admin account exists yet and AdminSetup:InitialPassword is not configured. " +
                "Set it in appsettings.json (or better, user secrets) for the first run, then you can remove it.");
        }

        var recoveryEmail = app.Configuration["AdminSetup:RecoveryEmail"] ?? "";

        db.AdminUsers.Add(new AdminUser
        {
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPassword),
            RecoveryEmail = recoveryEmail
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        ctx.Response.Headers.Remove("X-Frame-Options");
        ctx.Response.Headers["Content-Security-Policy"] =
            "frame-ancestors 'self' http://abusaeedportfolio.runasp.net https://abusaeedportfolio.runasp.net http://localhost:* https://localhost:*";
        return Task.CompletedTask;
    });
    await next();
});

app.UseDefaultFiles();   // serves wwwroot/index.html at "/"
app.UseStaticFiles();    // serves index.html, admin.html, assets, uploads/*
app.UseCors("PortfolioFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
