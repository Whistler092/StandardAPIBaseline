using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

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

    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         DELETE FROM ratings 
                                                                         WHERE movieid = @movieId 
                                                                            AND userid = @userId
                                                                         """, new { userId, movieId },
            cancellationToken: cancellationToken));
        return result > 0;
    }

    public async Task<IEnumerable<MovieRating>> GetRatingsForUserAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryAsync<MovieRating>(new CommandDefinition("""
                SELECT r.rating, r.movieid, m.slug
                FROM ratings r 
                INNER JOIN movies m on r.movieid = m.id
                WHERE userid = @userId
                """, new { userId },
            cancellationToken: cancellationToken));
        return result;
    }
}