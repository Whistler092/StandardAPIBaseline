namespace Movies.Contracts.Request;

public class GetAllMoviesRequest : PagedRequest
{
    public required string? Title { get; init; }

    public required int? Year { get; init; }

    public required string? SortBy { get; set; }
}