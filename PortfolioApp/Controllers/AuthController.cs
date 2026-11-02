using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Data;
using PortfolioApp.DTOs;
using PortfolioApp.Models;
using PortfolioApp.Services;
using System.Linq;

namespace PortfolioApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly PortfolioDbContext _db;
    private readonly TokenService _tokenService;
    private readonly EmailService _emailService;

    public AuthController(PortfolioDbContext db, TokenService tokenService, EmailService emailService)
    {
        _db = db;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Password is required." });

        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        if (admin == null)
            return StatusCode(500, new { message = "Admin account is not set up. See README for first-run setup." });

        bool valid = BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash);
        if (!valid)
            return Unauthorized(new { message = "Invalid password." });

        var (token, expiresAtUtc) = _tokenService.GenerateAdminToken();
        return Ok(new LoginResponse { Token = token, ExpiresAtUtc = expiresAtUtc });
    }

    // POST /api/auth/change-password — requires being logged in already.
    [HttpPost("change-password")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        if (admin == null)
            return StatusCode(500, new { message = "Admin account is not set up." });

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, admin.PasswordHash))
            return BadRequest(new { message = "Current password is incorrect." });

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password updated." });
    }

    // GET /api/auth/recovery-email — admin only. Shows the currently configured recovery address.
    [HttpGet("recovery-email")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRecoveryEmail()
    {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        return Ok(new { recoveryEmail = admin?.RecoveryEmail ?? "" });
    }

    // PUT /api/auth/recovery-email — admin only. Body: { "email": "..." }
    [HttpPut("recovery-email")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRecoveryEmail([FromBody] UpdateRecoveryEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            !System.Text.RegularExpressions.Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return BadRequest(new { message = "Please provide a valid email address." });

        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        if (admin == null) return StatusCode(500, new { message = "Admin account is not set up." });

        admin.RecoveryEmail = request.Email;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Recovery email updated." });
    }

    // POST /api/auth/forgot-password
    // Sends a 6-digit one-time code to the admin's recovery email.
    // Always returns the same generic message, whether or not sending succeeded internally,
    // so the endpoint doesn't reveal account existence to a caller who doesn't already know it.
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword()
    {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        if (admin == null || string.IsNullOrWhiteSpace(admin.RecoveryEmail))
            return Ok(new { message = "If an account is configured, a reset code has been sent." });

        // Generate a 6-digit numeric code.
        var code = Random.Shared.Next(100000, 999999).ToString();

        var resetEntry = new PasswordResetCode
        {
            CodeHash = BCrypt.Net.BCrypt.HashPassword(code),
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15),
            Used = false,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.PasswordResetCodes.Add(resetEntry);
        await _db.SaveChangesAsync();

        try
        {
            await _emailService.SendAsync(
                admin.RecoveryEmail,
                "Your portfolio admin password reset code",
                $@"<p>Your password reset code is:</p>
                   <h2 style='letter-spacing:4px'>{code}</h2>
                   <p>This code expires in 15 minutes. If you didn't request this, you can ignore this email.</p>"
            );
        }
        catch (Exception)
        {
            // Swallow the error from the caller's perspective (avoid account/email enumeration
            // and avoid leaking SMTP configuration issues to the public internet), but the
            // reset code row still exists and can be inspected server-side / logged if needed.
        }

        return Ok(new { message = "If an account is configured, a reset code has been sent." });
    }

    // POST /api/auth/reset-password
    // Body: { "code": "123456", "newPassword": "..." }
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "Code and new password are required." });

        if (request.NewPassword.Length < 8)
            return BadRequest(new { message = "New password must be at least 8 characters." });

        var admin = await _db.AdminUsers.FirstOrDefaultAsync();
        if (admin == null)
            return BadRequest(new { message = "Invalid or expired code." });

        // Find a still-valid, unused code whose hash matches what was supplied.
        var candidates = await _db.PasswordResetCodes
            .Where(c => !c.Used && c.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        var match = candidates.FirstOrDefault(c => BCrypt.Net.BCrypt.Verify(request.Code, c.CodeHash));
        if (match == null)
            return BadRequest(new { message = "Invalid or expired code." });

        match.Used = true;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Password has been reset. You can now log in." });
    }
}
