namespace NetScraper.Api;

public record CreateGroupDto(string Name);
public record GroupDto(int Id, string Name, int TermCount);

public record CreateTermDto(
    string Term, int SearchGroupId, DateTime? StartDate, DateTime? EndDate, string? OutputQuery);

public record TermDto(
    int Id, string Term, int SearchGroupId, DateTime? StartDate, DateTime? EndDate, string? OutputQuery);
