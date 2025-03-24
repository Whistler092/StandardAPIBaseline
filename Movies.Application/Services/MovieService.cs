using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IValidator<GetAllMoviesOptions> _optionsValidator;

    public MovieService(IMovieRepository movieRepository, IValidator<Movie> movieValidator,
        IRatingRepository ratingRepository, IValidator<GetAllMoviesOptions> optionsValidator)
    {
        _movieRepository = movieRepository;
        _movieValidator = movieValidator;
        _ratingRepository = ratingRepository;
        _optionsValidator = optionsValidator;
    }


    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetByIdAsync(id, userId, cancellationToken);
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options = default,
        CancellationToken cancellationToken = default)
    {
        await _optionsValidator.ValidateAndThrowAsync(options, cancellationToken);
        
        return await _movieRepository.GetAllAsync(options, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default,
        CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
   
        var exist = await _movieRepository.ExistByIdAsync(movie.Id, userId, cancellationToken);
        if (!exist)
            throw new KeyNotFoundException();

        await _movieRepository.UpdateAsync(movie, cancellationToken);
        
        if (!userId.HasValue)
        {
            var rating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }
        
        var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.userRating;
        
        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.DeleteByIdAsync(id, cancellationToken);
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetCountAsync(title, yearOfRelease, cancellationToken);
    }
}