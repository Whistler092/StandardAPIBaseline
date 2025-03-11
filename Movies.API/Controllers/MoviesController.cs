using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.API.Controllers;

[Authorize]
[ApiController] 
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> GetById([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
                ? await _movieService.GetByIdAsync(id, cancellationToken) 
                : await _movieService.GetBySlugAsync(idOrSlug, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }
        return Ok(movie.MapToToResponse());
    }
    
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var movies = await _movieService.GetAllAsync(cancellationToken);
        var moviesResponse = movies.ToMoviesResponse();
        return Ok(moviesResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var updated = await _movieService.UpdateAsync(movie, cancellationToken);
        if (updated is null)
            return NotFound();

        var response = updated.MapToToResponse();
        return Ok(response);
    }
    
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var updated = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!updated)
            return NotFound();

        return Ok();
    }
}