using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.API.Controllers;

[ApiController]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    public async Task<IActionResult> RateMovie([FromRoute] Guid id,
        [FromBody] RateMovieRequest movieRequest,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(id, movieRequest.Rating, userId.Value, cancellationToken);
        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpDelete(ApiEndpoints.Movies.DeleteRatings)]
    public async Task<IActionResult> DeleteRating([FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken);
        return result ? Ok() : NotFound();
    }

    [Authorize]
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatings(
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);

        var ratingsResponse = ratings.MapToResponse();
        return Ok(ratingsResponse);


    }
}