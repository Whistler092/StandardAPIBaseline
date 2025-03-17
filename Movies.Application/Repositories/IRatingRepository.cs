using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<bool> RateMoveAsync(Guid movieId, int rating, Guid userId, CancellationToken cancellationToken = default);
    
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken);

    Task<(float? Rating, int? userRating)> GetRatingAsync(Guid movieId, Guid userId,
        CancellationToken cancellationToken);
    
    Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}