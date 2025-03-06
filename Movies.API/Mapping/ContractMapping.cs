using Movies.Application.Models;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }
    
    public static MovieResponse MapToToResponse(this Movie movie)
    {
        return new MovieResponse
        {
            Id = Guid.NewGuid(),
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres.ToList()
        };
    }
}