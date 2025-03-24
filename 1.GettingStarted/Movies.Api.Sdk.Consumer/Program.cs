
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Movies.Api.Sdk;
using Movies.Contracts.Request;
using Refit;

//var moviesApi = RestService.For<IMoviesApi>("https://localhost:7001");

var services = new ServiceCollection();
services.AddRefitClient<IMoviesApi>(x => new RefitSettings
    {
        AuthorizationHeaderValueGetter = () => Task.FromResult("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlZDkwMTI4MC05M2E2LTQ4YjgtOTc0Zi1jOGIyMTMwZGVlZWIiLCJzdWIiOiJuaWNrQG5pY2tjaGFwc2FzLmNvbSIsImVtYWlsIjoibmlja0BuaWNrY2hhcHNhcy5jb20iLCJ1c2VyaWQiOiJkODU2NmRlMy1iMWE2LTRhOWItYjg0Mi04ZTM4ODdhODJlNDEiLCJhZG1pbiI6dHJ1ZSwidHJ1c3RlZF9tZW1iZXIiOnRydWUsIm5iZiI6MTc0Mjc4NDc5NiwiZXhwIjoxNzQyODEzNTk2LCJpYXQiOjE3NDI3ODQ3OTYsImlzcyI6Imh0dHBzOi8vaWQubmlja2NoYXBzYXMuY29tIiwiYXVkIjoiaHR0cHM6Ly9tb3ZpZXMubmlja2NoYXBzYXMuY29tIn0.DDZZi99isseqgnCm3QzNck812F9TeILzvIk9lcfdz3E")
    })
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7001"));
var provider = services.BuildServiceProvider();

var moviesApi = provider.GetRequiredService<IMoviesApi>();

var movie = await moviesApi.GetMovieAsync("fast--furious-2009");

Console.WriteLine(JsonSerializer.Serialize(movie, new JsonSerializerOptions { WriteIndented = true }));

/*var movies = await moviesApi.GetAllMoviesAsync(new GetAllMoviesRequest{
     Title = null,
     Year = null,
    SortBy = null,
    Page = 1,
    PageSize = 10
});

foreach (var m in movies.Items)
{
    Console.WriteLine(JsonSerializer.Serialize(m, new JsonSerializerOptions { WriteIndented = true }));
}*/