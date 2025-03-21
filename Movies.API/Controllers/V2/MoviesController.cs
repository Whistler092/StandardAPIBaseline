using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers.V2;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet(ApiEndpoints.V2.Movies.Get)]
    public async Task<IActionResult> GetById([FromRoute] string idOrSlug,
        [FromServices] LinkGenerator linkGenerator,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id)
            ? await _movieService.GetByIdAsync(id, userId, cancellationToken)
            : await _movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToToResponse();
        var movieObj = new
        {
            id = movie.Id,
        };
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Movies.API.Controllers.V2.MoviesController.GetById), values: new { idOrSlug = movie.Slug }),
            Rel = "self",
            Type = "GET"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Movies.API.Controllers.V1.MoviesController.Update), values: movieObj),
            Rel = "self",
            Type = "PUT"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Movies.API.Controllers.V1.MoviesController.Delete), values: movieObj),
            Rel = "self",
            Type = "DELETE"
        });

        return Ok(response);
    }

}