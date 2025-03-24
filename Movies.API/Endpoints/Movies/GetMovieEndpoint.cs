using System;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.API.Endpoints.Movies;

public static class GetMovieEndpoint
{
    public const string Name = "GetMovie";
    public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.Get,
            async (string idOrSlug, 
                IMovieService movieService, 
                HttpContext context,
                LinkGenerator linkGenerator,
                CancellationToken cancellationToken) => {

                var userId = context.GetUserId();

                var movie = Guid.TryParse(idOrSlug, out var id)
                    ? await movieService.GetByIdAsync(id, userId, cancellationToken)
                    : await movieService.GetBySlugAsync(idOrSlug, userId, cancellationToken);
                if (movie is null)
                {
                    return Results.NotFound();
                }

                var response = movie.MapToToResponse();
               
                /* 
                var movieObj = new
                {
                    id = movie.Id,
                };
                 response.Links.Add(new Link
                {
                    Href = linkGenerator.GetPathByAction(context, nameof(MapGetMovie), values: new { idOrSlug = movie.Slug }),
                    Rel = "self",
                    Type = "GET"
                });
                response.Links.Add(new Link
                {
                    Href = linkGenerator.GetPathByAction(context, nameof(Update), values: movieObj),
                    Rel = "self",
                    Type = "PUT"
                });
                response.Links.Add(new Link
                {
                    Href = linkGenerator.GetPathByAction(context, nameof(Delete), values: movieObj),
                    Rel = "self",
                    Type = "DELETE"
                });
 */
                return TypedResults.Ok(response);

            }).WithName(Name)
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        return app;
    }


    /*  [HttpGet(ApiEndpoints.Movies.Get)]
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
     } */

}
