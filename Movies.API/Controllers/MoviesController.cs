using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.API.Controllers;

[ApiController] 
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie);
        
        return CreatedAtAction(nameof(GetById), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetById([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
                ? await _movieService.GetByIdAsync(id) 
                : await _movieService.GetBySlugAsync(idOrSlug);
        if (movie is null)
        {
            return NotFound();
        }
        return Ok(movie.MapToToResponse());
    }
    
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieService.GetAllAsync();
        var moviesResponse = movies.ToMoviesResponse();
        return Ok(moviesResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updated = await _movieService.UpdateAsync(movie);
        if (updated is null)
            return NotFound();

        var response = updated.MapToToResponse();
        return Ok(response);
    }
    
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var updated = await _movieService.DeleteByIdAsync(id);
        if (!updated)
            return NotFound();

        return Ok();
    }
}