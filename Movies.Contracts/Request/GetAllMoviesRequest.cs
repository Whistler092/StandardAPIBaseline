namespace Movies.Contracts.Request;

public class GetAllMoviesRequest
{
    public required string? Title { get; init; }

    public required int? Year { get; init; }
}