namespace GameMarketIntel.Domain.Entities;

public sealed class Game
{
    private readonly List<Genre> _genres = [];
    
    private readonly List<Platform> _platforms = [];
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateOnly? ReleaseDate { get; private set; }

    public string? ImageUrl { get; private set; }


    public IReadOnlyCollection<Genre> Genres => _genres.AsReadOnly();
    public IReadOnlyCollection<Platform> Platforms =>_platforms.AsReadOnly();



    private Game()
    {
    }

    public Game(
        string name,
        string? description = null,
        DateOnly? releaseDate = null,
        string? imageUrl = null)
    {

        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("The game cannot be null, empty or whitespace",nameof(name));
        
        
        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = NormalizeOptionalText(description);
        ReleaseDate = releaseDate;
        ImageUrl = ValidateAndNormalizeOptionalUrl(imageUrl);


    }
    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? ValidateAndNormalizeOptionalUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))  return null;
 

        var isValidAbsoluteUrl = Uri.TryCreate(
            imageUrl.Trim(),
            UriKind.Absolute,
            out var parsedUrl
            );

        if (!isValidAbsoluteUrl || parsedUrl is null)
            throw new ArgumentException( "The game image URL must be a valid absolute URL.",nameof(imageUrl));
        

        var isHttpOrHttps = parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps;

        if (!isHttpOrHttps)
            throw new ArgumentException( "The game image URL must use HTTP or HTTPS.", nameof(imageUrl));
        

        return parsedUrl.ToString();
    }
    public void AddGenre(Genre genre)
    {
        ArgumentNullException.ThrowIfNull(genre);

        if (_genres.Any(existingGenre => existingGenre.Id == genre.Id)) return;

        _genres.Add(genre);
    }
    public void AddPlatform(Platform platform)
    {
        ArgumentNullException.ThrowIfNull(platform);

        if (_platforms.Any(existingPlatform => existingPlatform.Id == platform.Id))   return;

        _platforms.Add(platform);
    }

}