using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnection;

    public MovieRepository(IDbConnectionFactory dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        //Since I am going to insert into two tables (Movies and Genres), we need to use a transaction
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into Movies (id, slug, title, yearofrelease)
                                                                         values (@id, @slug, @title, @yearofrelease)
                                                                         """, movie,
            cancellationToken: cancellationToken));
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                                                                    insert into genres (movieId, name)
                                                                    values (@movieId, @name)
                                                                    """, new { MovieId = movie.Id, Name = genre },
                    cancellationToken: cancellationToken));
            }
        }

        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid movieId, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  select m.* , 
                                         round(avg(r.rating), 1) as rating,
                                         myr.rating as userrating
                                  from Movies m
                                  left join ratings r on m.id = r.movieid
                                  left join ratings myr on m.id = myr.movieid 
                                                               and myr.userid = @userid 
                                  where m.id = @id
                                  group by id, myr.rating
                                  """,
                new { id = movieId, userId }, cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieId = @id
                                  """, new { id = movieId }, cancellationToken: cancellationToken));
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""
                                  select m.* , 
                                         round(avg(r.rating), 1) as rating,
                                         myr.rating as userrating
                                  from Movies m
                                  left join ratings r on m.id = r.movieid
                                  left join ratings myr on m.id = myr.movieid 
                                                               and myr.userid = @userid 
                                  where m.slug = @slug
                                  group by id, myr.rating
                                  """,
                new { slug, userId }, cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieId = @id
                                  """, new { id = movie.Id }, cancellationToken: cancellationToken));
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);

        var orderClause = string.Empty;
        if (options.SortField is not null)
        {
            orderClause = $"""
                           , m.{options.SortField}
                           order by m.{options.SortField} {(options.SortOrder == SortOrder.Ascending ? "asc" : "desc")}
                           """;
        }

        var result = await connection.QueryAsync(
            new CommandDefinition($"""
                                   select m.*,
                                          string_agg(g.name, ',') as genres,
                                          round(avg(r.rating), 1) as rating,
                                          myr.rating as userrating
                                   from Movies m
                                   left join genres g on m.id = g.movieId
                                   left join ratings r on m.id = r.movieid
                                   left join ratings myr on m.id = myr.movieid 
                                                                and myr.userid = @userid 
                                   WHERE (@title is null or m.title like ('%' || @title || '%'))
                                   AND (@yearofrelease is null or m.yearofrelease = @yearofrelease)
                                   group by id, myr.rating {orderClause}
                                   limit @pageSize
                                   offset @pageOffset
                                   """, new
            {
                userId = options.UserId,
                title = options.Title,
                yearofrelease = options.YearOfRelease,
                pageSize = options.PageSize,
                pageOffset = (options.Page - 1) * options.PageSize,
            }, cancellationToken: cancellationToken));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            Rating = (float?)x.rating,
            UserRating = (int?)x.userrating,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(',')),
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres where movieId = @id
            """, new { id = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO genres (movieId, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE movies set slug = @slug, title = @title, yearOfRelease = @yearOfRelease
            """, movie, cancellationToken: cancellationToken));
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres where movieId = @id
            """, new { id = id }, cancellationToken: cancellationToken));

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from movies where id = @id
            """, new { id }, cancellationToken: cancellationToken));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                                                                               select count(1) 
                                                                               from Movies where id = @id
                                                                               """, new { id },
            cancellationToken: cancellationToken));
    }

    public async Task<int> GetCountAsync(string? title, int? yearOfRelease,
        CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnection.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<int>(
            new CommandDefinition("""
                                  SELECT count(id)
                                  FROM movies m
                                  WHERE (@title is null or m.title like ('%' || @title || '%'))
                                  AND (@yearofrelease is null or m.yearofrelease = @yearofrelease)
                                  """, new
            {
                title,
                yearOfRelease
            }, cancellationToken: cancellationToken));
    }
}