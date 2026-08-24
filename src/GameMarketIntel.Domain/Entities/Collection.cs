namespace GameMarketIntel.Domain.Entities;

public sealed class Collection
{
    public const int MaxNameLength = 200;

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    private Collection()
    {
    }

    public Collection(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "The collection name cannot be null, empty or whitespace.",
                nameof(name));

        var normalizedName = name.Trim();

        if (normalizedName.Length > MaxNameLength)
            throw new ArgumentException(
                $"The collection name cannot exceed {MaxNameLength} characters.",
                nameof(name));

        Id = Guid.NewGuid();
        Name = normalizedName;
        NormalizedName = normalizedName.ToUpperInvariant();
    }
}