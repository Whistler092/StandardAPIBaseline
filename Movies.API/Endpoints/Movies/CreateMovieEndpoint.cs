using System;
using Microsoft.AspNetCore.OutputCaching;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Request;
using Movies.Contracts.Responses;

namespace Movies.API.Endpoints.Movies;

public static class CreateMovieEndpoint
{
    public const string Name = "CreateMovie";

    public static IEndpointRouteBuilder MapCreateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiEndpoints.Movies.Create,  async(
            CreateMovieRequest request,
            IMovieService movieService,
            IOutputCacheStore _outputCacheStore,
            CancellationToken cancellationToken) => {


            var movie = request.MapToMovie();
            await movieService.CreateAsync(movie, cancellationToken);

            await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

            var response = movie.MapToToResponse();
            return TypedResults.CreatedAtRoute(response, GetMovieEndpoint.Name, new { idOrSlug = movie.Slug });
            //return CreatedAtAction(nameof(GetByIdV1), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        })
            .WithName(Name)
            .Produces<MovieResponse>(StatusCodes.Status201Created)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(AuthConstants.TruestedMemberPolicyName);

        return app;
    }

    /*
     [Authorize(AuthConstants.TruestedMemberPolicyName)]
    //[ServiceFilter(typeof(ApiKeyAuthFilter))] Only for token based auth
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request,
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);

        await _outputCacheStore.EvictByTagAsync("movies", cancellationToken);

        //return CreatedAtAction(nameof(GetByIdV1), new { idOrSlug = movie.Slug }, movie.MapToToResponse());
        return Created($"/{ApiEndpoints.Movies.Create}/{movie.Id}", movie.MapToToResponse());
    }
    */

}
