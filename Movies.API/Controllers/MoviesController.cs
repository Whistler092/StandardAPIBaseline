using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers;

[ApiController]
[ApiVersion(1.0)] 
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IOutputCacheStore _outputCacheStore;

    public MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore)
    {
        _movieService = movieService;
        _outputCacheStore = outputCacheStore;
    }

    [Authorize(AuthConstants.TruestedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        return CreatedAtAction(nameof(GetByIdV1), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        //return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [OutputCache(PolicyName = "MovieCache")]
    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdV1([FromRoute] string idOrSlug,
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
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetByIdV1), values: new { idOrSlug = movie.Slug }),
            Rel = "self",
            Type = "GET"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Update), values: movieObj),
            Rel = "self",
            Type = "PUT"
        });
        response.Links.Add(new Link
        {
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), values: movieObj),
            Rel = "self",
            Type = "DELETE"
        });

        return Ok(response);
    }
     
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MovieCache")]
    //[ResponseCache(Duration = 30, VaryByQueryKeys = new [] { "title", "year", "sortBy", "page", "PageSize" }, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetAllMoviesRequest request,
        CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions()
            .WithUser(userId);
        var movies = await _movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, cancellationToken);
        var moviesResponse = movies.ToMoviesResponse(request.Page, request.PageSize, movieCount);
        return Ok(moviesResponse);
    }

    [Authorize(AuthConstants.TruestedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var userId = HttpContext.GetUserId();

        var updated = await _movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updated is null)
            return NotFound();

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        var response = updated.MapToToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var updated = await _movieService.DeleteByIdAsync(id, cancellationToken);
        if (!updated)
            return NotFound();
        
        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return Ok();
    }
}