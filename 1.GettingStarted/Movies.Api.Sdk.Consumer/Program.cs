
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Contracts.Request;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("https://localhost:7001");

var services = new ServiceCollection();

services.AddRefitClient<IMoviesApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7001"));
var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

//var movie = await moviesApi.GetMovieAsync("fast--furious-2009");

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