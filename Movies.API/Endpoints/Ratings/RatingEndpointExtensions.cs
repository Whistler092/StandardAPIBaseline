using System;

namespace Movies.API.Endpoints.Ratings;

public static class RatingEndpointExtensions
{
    public static IEndpointRouteBuilder MapRatingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetUserRatings();
        app.MapRateMovie();
        app.MapDeleteRating();
        return app;
    }

}
