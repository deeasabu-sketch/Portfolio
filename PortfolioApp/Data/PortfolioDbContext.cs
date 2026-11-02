using Microsoft.EntityFrameworkCore;
using PortfolioApp.Models;

namespace PortfolioApp.Data;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) { }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ProfileLink> ProfileLinks => Set<ProfileLink>();
    public DbSet<SiteProfile> SiteProfiles => Set<SiteProfile>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed a default set of links so the site isn't empty on first run.
        // Replace these from the admin panel after your first login.
        modelBuilder.Entity<ProfileLink>().HasData(
            new ProfileLink { Id = 1, Label = "GitHub", Url = "https://github.com/", Type = "github", SortOrder = 1 },
            new ProfileLink { Id = 2, Label = "LinkedIn", Url = "https://www.linkedin.com/in/abu-saeed-6347692b5", Type = "linkedin", SortOrder = 2 },
            new ProfileLink { Id = 3, Label = "Website", Url = "https://example.com", Type = "website", SortOrder = 3 }
        );
        modelBuilder.Entity<Skill>()
                .ToTable("Skills");

        modelBuilder.Entity<SiteProfile>().HasData(
            new SiteProfile { Id = 1, CvFileName = null, CvStoredPath = null, CvUpdatedAt = null }
        );
    }
}
