namespace PortfolioApp.Models;

// A project card shown in the "Projects" section. Managed from the admin panel.
public class Project
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;    // comma separated, e.g. "C#, SQL, ADO.NET"
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public int SortOrder { get; set; }
}
