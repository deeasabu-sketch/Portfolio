namespace PortfolioApp.DTOs;

public class SkillDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int Level { get; set; }
    public int SortOrder { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public int SortOrder { get; set; }
}

public class SiteTextDto
{
    public string? HeroTitle { get; set; }
    public string? HeroText { get; set; }
    public string? AboutText { get; set; }
}

public class PhotoInfoDto
{
    public string Url { get; set; } = "assets/profile.jpg";
    public bool IsCustom { get; set; }
}
