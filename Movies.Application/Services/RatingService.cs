using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRepository _movieRepository;

    public RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository)
    {
        _ratingRepository = ratingRepository;
        _movieRepository = movieRepository;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (rating is <= 0 or > 5)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure
                {
                    PropertyName = "Rating",
                    ErrorMessage = "Rating must be between 0 and 5"
                }
            });
        }

        var movieExist = await _movieRepository.ExistByIdAsync(movieId, userId, cancellationToken);
        if (!movieExist)
        {
            return false;
        }

        return await _ratingRepository.RateMoveAsync(movieId, rating, userId, cancellationToken);
    }

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _ratingRepository.GetRatingsForUserAsync(userId, cancellationToken);
    }
}