using System;
using Microsoft.AspNetCore.OutputCaching;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;

namespace Movies.API.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
            Guid id,
            UpdateMovieRequest request,
            IMovieService movieService,
            HttpContext context,
            IOutputCacheStore _outputCacheStore,
            CancellationToken cancellationToken) =>
        {

            var movie = request.MapToMovie(id);
            var userId = context.GetUserId();

            var updated = await movieService.UpdateAsync(movie, userId, cancellationToken);
            if (updated is null)
                return Results.NotFound();

            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
            var response = updated.MapToToResponse();
            return TypedResults.CreatedAtRoute(response, GetMovieEndpoint.Name, new { idOrSlug = movie.Slug });
            //return CreatedAtAction(nameof(GetByIdV1), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        })
            .WithName(Name);

        return app;
    }

    /*
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
    }*/

}
