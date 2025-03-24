
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Movies.Contracts.Request;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("https://localhost:7001");

var services = new ServiceCollection();
services
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(x => new RefitSettings
    {
        AuthorizationHeaderValueGetter = async () => await x.GetRequiredService<AuthTokenProvider>().GetTokenAsync()
    })
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7001"));
var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var moviesInitial = await moviesApi.GetAllMoviesAsync(new GetAllMoviesRequest{
    Title = "Back",
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var m in moviesInitial.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true }));

    await moviesApi.DeleteMovieAsync(m.Id);
}

/* var movie = await moviesApi.GetMovieAsync("fast--furious-2009");

Console.WriteLine(JsonSerializer.Serialize(movie, new JsonSerializerOptions { WriteIndented = true }));
 */

// Create a movie
/*
{
    "original_title": "Back to the Future",
    "release_date": "1985-07-03",
},
{
    "original_title": "Back to the Future Part II",
    "release_date": "1989-11-22",
},
{
    "original_title": "Back to the Future Part III",
    "release_date": "1990-05-25",
},
*/


var newMovie = await moviesApi.CreateMovieAsync(new CreateMovieRequest
{
    Title = "Back to the Future",
    YearOfRelease = 1985,
    Genres = new List<string> { "Adventure", "Comedy", "Science Fiction" }
});

Console.WriteLine(JsonSerializer.Serialize(newMovie, new JsonSerializerOptions { WriteIndented = true }));

// Update a movie
var updatedMovie = await moviesApi.UpdateMovieAsync(newMovie.Id, new UpdateMovieRequest
{
    
    Title = "Back to the Future Part II",
    YearOfRelease = 1989,
    Genres = new List<string> { "Adventure", "Comedy", "Science Fiction" }
});

Console.WriteLine(JsonSerializer.Serialize(updatedMovie, new JsonSerializerOptions { WriteIndented = true }));

// Delete movie  
await moviesApi.DeleteMovieAsync(updatedMovie.Id);

var movies = await moviesApi.GetAllMoviesAsync(new GetAllMoviesRequest{
    Title = "Back",
    Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var m in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true }));
}


