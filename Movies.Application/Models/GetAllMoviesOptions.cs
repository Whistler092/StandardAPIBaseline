namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public string? Title { get; set; }
    public int? YearOfRelease { get; set; }
    public Guid? UserId { get; set; }
    
    public string? SortField { get; set; }
    public SortOrder? SortOrder { get; set; }
}

public enum SortOrder
{
    Ascending,
    Descending,
    Unsorted
}