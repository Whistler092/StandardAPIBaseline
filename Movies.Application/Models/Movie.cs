using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; set; }
    public required string Title { get; init; }

    public string Slug => GenerateSlug();

    public required int YearOfRelease { get; init; }
    public required IEnumerable<string> Genres { get; init; } = []; //Enumerable.Empty<string>();

    private string GenerateSlug()
    {
        var sluggledTitle = SlugRegex().Replace(Title, String.Empty)
            .ToLower().Replace(" ", "-");
        return $"{sluggledTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
    private static partial Regex SlugRegex();
}