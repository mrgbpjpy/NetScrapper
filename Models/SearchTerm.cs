namespace NetScraper.Api.Models;

public class SearchTerm
{
    public int Id { get; set; }
    public string Term { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OutputQuery { get; set; }

    public int SearchGroupId { get; set; }
    public SearchGroup? SearchGroup { get; set; }
}
