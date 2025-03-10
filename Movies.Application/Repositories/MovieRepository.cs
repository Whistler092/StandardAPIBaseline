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

    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();

        //Since I am going to insert into two tables (Movies and Genres), we need to use a transaction
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition("""
                                                                         insert into Movies (id, slug, title, yearofrelease)
                                                                         values (@id, @slug, @title, @yearofrelease)
                                                                         """, movie));
        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                                                                    insert into genres (movieId, name)
                                                                    values (@movieId, @name)
                                                                    """, new { MovieId = movie.Id, Name = genre }));
            }
        }

        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""select * from Movies where id = @id""",
                new { MovieId = id }));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieId = @id
                                  """, new { id }));
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new CommandDefinition("""select * from Movies where slug = @slug""",
                new { slug }));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new CommandDefinition("""
                                  select name from genres where movieId = @id
                                  """, new { id = movie.Id }));
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        var result = await connection.QueryAsync(
            new CommandDefinition("""
                                  select m.*, string_agg(g.name, ',') as genres 
                                  from Movies m
                                  left join genres g on m.id = g.movieId
                                  group by id
                                  """));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(',')),
        });
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var exist = await ExistByIdAsync(movie.Id);
        if (!exist)
            throw new KeyNotFoundException();
        
        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres where movieId = @id
            """, new { id = movie.Id }));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                """
                INSERT INTO genres (movieId, name)
                VALUES (@MovieId, @Name)
                """, new { MovieId = movie.Id, Name = genre }));
        }

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            UPDATE movies set slug = @slug, title = @title, yearOfRelease = @yearOfRelease
            """, movie));
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from genres where movieId = @id
            """, new { id = id }));

        var result = await connection.ExecuteAsync(new CommandDefinition(
            """
            delete from movies where id = @id
            """, new { id }));

        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistByIdAsync(Guid id)
    {
        using var connection = await _dbConnection.CreateConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                                                                               select count(1) 
                                                                               from Movies where id = @id
                                                                               """, new { id }));
    }
}