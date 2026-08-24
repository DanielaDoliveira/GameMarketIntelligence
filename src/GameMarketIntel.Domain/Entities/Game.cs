using GameMarketIntel.Domain.Enums;

namespace GameMarketIntel.Domain.Entities;

public sealed class Game
{
    private readonly List<Genre> _genres = [];

    private readonly List<Platform> _platforms = [];

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateOnly? FirstReleaseDate { get; private set; }

    public GameProductType? ProductType { get; private set; }

    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();

    public IReadOnlyCollection<Platform> Platforms => _platforms.AsReadOnly();

    private Game()
    {
    }

    public Game(
        string name,
        string? description = null,
        DateOnly? firstReleaseDate = null,
        GameProductType? productType = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "The game cannot be null, empty or whitespace",
                nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        NormalizedName = Name.ToUpperInvariant();
        Description = NormalizeOptionalText(description);
        FirstReleaseDate = firstReleaseDate;
        ProductType = productType;
    }

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public void AddGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        if (_genres.Any(existingGenre => existingGenre.Id == genre.Id))
            return;

        _genres.Add(genre);
    }

    public void AddPlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        if (_platforms.Any(existingPlatform => existingPlatform.Id == platform.Id))
            return;

        _platforms.Add(platform);
    }
}