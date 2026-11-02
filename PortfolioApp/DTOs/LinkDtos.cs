namespace PortfolioApp.DTOs;

public class LinkDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "custom";
    public int SortOrder { get; set; }
}

public class UpsertLinkRequest
{
    public int? Id { get; set; }              // null/0 = create new, otherwise update existing
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "custom";
    public int SortOrder { get; set; }
}

public class ReorderLinksRequest
{
    // Ordered list of link IDs, in the order they should appear.
    public List<int> OrderedIds { get; set; } = new();
}
