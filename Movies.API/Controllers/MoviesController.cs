using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Controller;

[ApiController]
[Route("api/")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;

    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    [HttpPost("movies")]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieRepository.CreateAsync(movie);
        return Created($"/api/movies/{movie.Id}", movie.MapToToResponse());
    }

    [HttpGet("movies")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _movieRepository.GetAllAsync());
    }
}