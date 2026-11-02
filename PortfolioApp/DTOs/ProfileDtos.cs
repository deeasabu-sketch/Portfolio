namespace PortfolioApp.DTOs;

public class CvInfoDto
{
    public bool HasCv { get; set; }
    public string? FileName { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? DownloadUrl { get; set; }
}
