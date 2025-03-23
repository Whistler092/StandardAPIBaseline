using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; set; }
    public required string Title { get; init; }

    public string Slug => GenerateSlug();

    public float? Rating { get; set; }

    public int? UserRating { get; set; }

    public required int YearOfRelease { get; init; }
    public required List<string> Genres { get; init; } = new(); //Enumerable.Empty<string>();

    private string GenerateSlug()
    {
        var sluggledTitle = SlugRegex().Replace(Title, String.Empty)
            .ToLower().Replace(" ", "-");
        return $"{sluggledTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z_-]", RegexOptions.NonBacktracking, 50)]
    private static partial Regex SlugRegex();
}