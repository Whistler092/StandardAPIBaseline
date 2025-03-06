using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Controller;

[ApiController] 
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieRepository.CreateAsync(movie);
        
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie.MapToToResponse());
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        if (movie is null)
        {
            return NotFound();
        }
        return Ok(movie.MapToToResponse());
    }
    
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieRepository.GetAllAsync();
        var moviesResponse = movies.ToMoviesResponse();
        return Ok(moviesResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updated = await _movieRepository.UpdateAsync(movie);
        if (!updated)
            return NotFound();

        var response = movie.MapToToResponse();
        return Ok(response);
    }
    
    [HttpPut(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var updated = await _movieRepository.DeleteByIdAsync(id);
        if (!updated)
            return NotFound();

        return Ok();
    }
}