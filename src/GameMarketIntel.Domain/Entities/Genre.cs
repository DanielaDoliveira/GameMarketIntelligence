
namespace GameMarketIntel.Domain.Entities;

public sealed class Genre
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public Genre(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Genre name cannot be null, empty or whitespace");

        Id = Guid.NewGuid();
        Name = name.Trim();
    }
}
