namespace PortfolioApp.Models;

// One row per link (GitHub, LinkedIn, Website, Facebook, YouTube, Gmail, a GitHub repo, etc.)
// "Type" lets the frontend pick the right icon; "SortOrder" controls display order.
public class ProfileLink
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;      // e.g. "GitHub", "LinkedIn", "SimpleStore Repo"
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "custom";            // github | linkedin | website | facebook | youtube | gmail | repo | custom
    public int SortOrder { get; set; }
}
