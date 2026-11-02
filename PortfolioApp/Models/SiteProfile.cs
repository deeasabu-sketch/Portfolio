namespace PortfolioApp.Models;

// Single-row table holding the current CV file info and a few editable
// headline fields, so updates from the admin panel reflect on the public site.
public class SiteProfile
{
    public int Id { get; set; }
    public string? CvFileName { get; set; }      // original file name shown to visitors, e.g. "Abu_Saeed_CV.pdf"
    public string? CvStoredPath { get; set; }     // physical path under wwwroot/uploads
    public DateTime? CvUpdatedAt { get; set; }

    // Profile photo uploaded from the admin panel (falls back to assets/profile.jpg when empty)
    public string? PhotoStoredPath { get; set; }
    public DateTime? PhotoUpdatedAt { get; set; }

    // Editable homepage text
    public string? HeroTitle { get; set; }
    public string? HeroText { get; set; }
    public string? AboutText { get; set; }
}
