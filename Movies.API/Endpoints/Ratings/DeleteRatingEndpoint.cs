using System;
using Movies.Application.Services;

namespace Movies.API.Endpoints.Ratings;

public static class DeleteRatingEndpoint
{

    public const string Name = "DeleteRating";

    public static IEndpointRouteBuilder MapDeleteRating(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.DeleteRatings, async (
            Guid id,
            HttpContext context,
            IRatingService ratingService,
            CancellationToken cancellationToken) =>
        {
            var userId = context.GetUserId();
            var result = await ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken);
            return result ? Results.Ok() : Results.NotFound();
        })
            .WithName(Name)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }

    /*    [Authorize]
   [HttpDelete(ApiEndpoints.Movies.DeleteRatings)]
   [ProducesResponseType(StatusCodes.Status200OK)]
   [ProducesResponseType(StatusCodes.Status404NotFound)]
   public async Task<IActionResult> DeleteRating([FromRoute] Guid id,
       CancellationToken cancellationToken = default)
   {
       var userId = HttpContext.GetUserId();
       var result = await _ratingService.DeleteRatingAsync(id, userId!.Value, cancellationToken);
       return result ? Ok() : NotFound();
   } */

}
