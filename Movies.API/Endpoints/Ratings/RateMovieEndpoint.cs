using System;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.API.Endpoints.Ratings;

public static class RateMovieEndpoint
{

    public const string Name = "RateMovie";
    public static IEndpointRouteBuilder MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Rate,
            async (
                Guid id, RateMovieRequest movieRequest,
                HttpContext context, IRatingService ratingService,
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var result = await ratingService.RateMovieAsync(id, movieRequest.Rating, userId.Value, cancellationToken);
                //return result ? Ok() : NotFound();
                return result ? Results.Ok() : Results.NotFound();

            }).WithName(Name)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
        return app;
    }

    /* [Authorize]
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateMovie([FromRoute] Guid id,
        [FromBody] RateMovieRequest movieRequest,
        CancellationToken cancellationToken = default)
    {
        var userId = HttpContext.GetUserId();
        var result = await _ratingService.RateMovieAsync(id, movieRequest.Rating, userId.Value, cancellationToken);
        return result ? Ok() : NotFound();
    }
 */
}
