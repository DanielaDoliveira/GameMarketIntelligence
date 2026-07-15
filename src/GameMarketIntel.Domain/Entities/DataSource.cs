using GameMarketIntel.Domain.ValueObjects;

namespace GameMarketIntel.Domain.Entities;

public sealed class DataSource
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string? LicenseNotes { get; private set; }

    public bool AttributionRequired { get; private set; }

    public SourceReliability Reliability { get; private set; } = null!;

    private DataSource()
    {
    }

    public DataSource(string name, string url, SourceReliability reliability, bool attributionRequired, string? licenseNotes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The data source name is required.", nameof(name));


        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("The data source URL is required.", nameof(url));


        var parsedUrl = ValidateAndParseUrl(url);

        ArgumentNullException.ThrowIfNull(reliability);

        Id = Guid.NewGuid();

        Name = name.Trim();

        Url = parsedUrl.ToString();

        Reliability = reliability;

        AttributionRequired = attributionRequired;

        LicenseNotes = string.IsNullOrWhiteSpace(licenseNotes) ? null : licenseNotes.Trim();
    }

    private static Uri ValidateAndParseUrl(string url)
    {
        var isValidAbsoluteUrl = Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl);

        if (!isValidAbsoluteUrl || parsedUrl is null)
            throw new ArgumentException ( "The data source URL must be a valid absolute URL.", nameof(url));


        var isHttpOrHttps = parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps;

        if (!isHttpOrHttps)
            throw new ArgumentException("The data source URL must use HTTP or HTTPS.", nameof(url));


        return parsedUrl;
    }
}