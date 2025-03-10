using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService : IMovieService
{
    
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }
    

    public async Task<bool> CreateAsync(Movie movie)
    {
        return await _movieRepository.CreateAsync(movie);
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _movieRepository.GetByIdAsync(id);
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        return await _movieRepository.GetBySlugAsync(slug);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _movieRepository.GetAllAsync();
    }

    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        var movieExist = await _movieRepository.ExistByIdAsync(movie.Id);
        if (!movieExist)
        {
            return null;
        }
        
        await _movieRepository.UpdateAsync(movie);
        return movie;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        return await _movieRepository.DeleteByIdAsync(id);  
    }
}