
using System.Text.Json;
using Movies.Api.Sdk;
using Movies.Contracts.Request;
using Refit;

var moviesApi = RestService.For<IMoviesApi>("https://localhost:7001");

var movie = await moviesApi.GetMovieAsync("fast--furious-2009");

//Console.WriteLine(JsonSerializer.Serialize(movie, new JsonSerializerOptions { WriteIndented = true }));

var movies = await moviesApi.GetAllMoviesAsync(new GetAllMoviesRequest{
     Title = null,
     Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var m in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true }));
}