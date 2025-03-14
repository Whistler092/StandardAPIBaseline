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
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            Rating = movie.Rating,
            UserRating = movie.UserRating,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres.ToList()
        };
    }

    public static MoviesResponse ToMoviesResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse()
        {
            Items = movies.Select(MapToToResponse)
        };
    }
    
    public static Movie MapToMovie(this UpdateMovieRequest request, Guid movieId)
    {
        return new Movie
        {
            Id = movieId,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList()
        };
    }
    
    
}