using Movies.API.Mapping;
using Movies.Application.Services;

namespace Movies.API.Endpoints.Ratings;

public static class GetUserRatingsEndpoint
{

    public const string Name = "GetUserRatings";
    public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetUserRatings,
            async (
                IRatingService ratingService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var ratings = await ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);

                var ratingsResponse = ratings.MapToResponse();

                return TypedResults.Ok(ratingsResponse);

            }).WithName(Name);
        return app;
    }


    /*  [Authorize]
        [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
        [ProducesResponseType(typeof(IEnumerable<MovieRatingResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserRatings(
            CancellationToken cancellationToken = default)
        {
            var userId = HttpContext.GetUserId();
            var ratings = await _ratingService.GetRatingsForUserAsync(userId!.Value, cancellationToken);

            var ratingsResponse = ratings.MapToResponse();
            return Ok(ratingsResponse);


        } */
}
