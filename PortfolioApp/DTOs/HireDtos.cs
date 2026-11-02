namespace PortfolioApp.DTOs;

public class HireRequestDto
{
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;   // e.g. "Website", "Web App", "Desktop App", "Other"
    public string Details { get; set; } = string.Empty;
    public bool WantsSample { get; set; }
    public string? Budget { get; set; }
}
