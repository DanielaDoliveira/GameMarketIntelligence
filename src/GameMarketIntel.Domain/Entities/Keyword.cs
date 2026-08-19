using GameMarketIntel.Domain.Common;

namespace GameMarketIntel.Domain.Entities;

public sealed class Keyword
{
    public const int MaxNameLength = 200;
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;

    private Keyword() { }

    public Keyword(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException
            (
                "The keyword name cannot be null, empty or whitespace.",
                nameof(name)
            );
        var normalizedDisplayName = name.Trim();
        if (normalizedDisplayName.Length > MaxNameLength)
            throw new ArgumentException
            (
                $"The keyword name cannot exceed {MaxNameLength} characters.",
                nameof(name)
            );
        Id = Guid.NewGuid();
        Name = normalizedDisplayName;
        NormalizedName = NameNormalizer.Normalize(normalizedDisplayName);

    }
}