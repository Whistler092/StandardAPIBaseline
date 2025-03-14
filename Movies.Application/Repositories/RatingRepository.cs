using Dapper;
using Movies.Application.Database;

namespace Movies.Application.Repositories;

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnection;

    public RatingRepository(IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> RateMoveAsync(Guid movieId, int rating, Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(
            new CommandDefinition("""
                     INSERT INTO ratings (userid, movieid, rating)
                     VALUES (@userid, @movieid, @rating)
                     ON CONFLICT (userid, movieid) DO UPDATE
                        SET rating = @rating
                     """, new { userId, movieId, rating },
            cancellationToken: cancellationToken));
        
        return result > 0;
    }

    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<float?>(new CommandDefinition("""
            SELECT 
                round(avg(r.rating), 1)
            FROM ratings r 
            WHERE movieid = @movieId
            """, new { movieId }, cancellationToken: cancellationToken));
    }

    public async Task<(float? Rating, int? userRating)> GetRatingAsync(Guid movieId, Guid userId,
        CancellationToken cancellationToken)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<(float?, int?)>(new CommandDefinition("""
            SELECT 
                round(avg(r.rating), 1),
                (SELECT rating from ratings r where r.movieid = @movieId and r.userid = @userId limit 1)
            FROM ratings r 
            WHERE movieid = @movieId
            """, new { movieId, userId }, cancellationToken: cancellationToken));
    }
}