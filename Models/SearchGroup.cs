namespace NetScraper.Api.Models;

public class SearchGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ICollection<SearchTerm> Terms { get; set; } = new List<SearchTerm>();
}
