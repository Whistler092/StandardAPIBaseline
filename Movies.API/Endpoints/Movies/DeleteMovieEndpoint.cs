using System;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Application.Services;

namespace Movies.API.Endpoints.Movies;

public static class DeleteMovieEndpoint
{

    public const string Name = "DeleteMovie";

    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
            Guid id,
            IMovieService movieService,
            IOutputCacheStore _outputCacheStore,
            CancellationToken cancellationToken) =>
        {
             var updated = await movieService.DeleteByIdAsync(id, cancellationToken);
            if (!updated)
                return Results.NotFound();

            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);
            return Results.Ok();
            //return CreatedAtAction(nameof(GetByIdV1), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        })
            .WithName(Name)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthConstants.AdminUserPolicyName);

        return app;
    }

    /* [Authorize(AuthConstants.AdminUserPolicyName)]
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
        } */
}
