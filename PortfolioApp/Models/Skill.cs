namespace PortfolioApp.Models;

// A skill card shown in the "Skills" section. Managed from the admin panel.
public class Skill
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;        // e.g. "C# / .NET"
    public string Description { get; set; } = string.Empty;  // short line under the title
    public string Details { get; set; } = string.Empty;      // longer text shown on skills.html
    public int Level { get; set; }                            // proficiency 0-100, drives the progress bar
    public int SortOrder { get; set; }
}
