namespace GameMarketIntel.Domain.Entities;

public sealed class Platform
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Family { get; private set; }

    public string? Manufacturer { get; private set; }

    public string? ImageUrl { get; private set; }

    private Platform()
    {
    }

    public Platform(
        string name,
        string? family = null,
        string? manufacturer = null,
        string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The platform name cannot be null, empty, or whitespace.",
                nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        Family = NormalizeOptionalText(family);
        Manufacturer = NormalizeOptionalText(manufacturer);
        ImageUrl = ValidateAndNormalizeOptionalUrl(imageUrl);
    }

    private static string? ValidateAndNormalizeOptionalUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return null;
        }

        var isValidAbsoluteUrl = Uri.TryCreate(
            imageUrl.Trim(),
            UriKind.Absolute,
            out var parsedUrl);

        if (!isValidAbsoluteUrl || parsedUrl is null)
        {
            throw new ArgumentException(
                "The platform image URL must be a valid absolute URL.",
                nameof(imageUrl));
        }

        var isHttpOrHttps =
            parsedUrl.Scheme == Uri.UriSchemeHttp ||
            parsedUrl.Scheme == Uri.UriSchemeHttps;

        if (!isHttpOrHttps)
        {
            throw new ArgumentException(
                "The platform image URL must use HTTP or HTTPS.",
                nameof(imageUrl));
        }

        return parsedUrl.ToString();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}