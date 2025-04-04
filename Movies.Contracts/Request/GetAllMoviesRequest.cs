namespace Movies.Contracts.Request;

public class GetAllMoviesRequest : PagedRequest
{
    public string? Title { get; init; }

    public int? Year { get; init; }

    public string? SortBy { get; set; }
}