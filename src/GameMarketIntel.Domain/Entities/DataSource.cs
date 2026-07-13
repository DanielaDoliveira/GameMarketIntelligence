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
            throw new ArgumentException("O nome da fonte é obrigatório.", nameof(name));


        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("A URL da fonte é obrigatória.", nameof(url));


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
            throw new ArgumentException("A URL da fonte deve ser uma URL absoluta válida.", nameof(url));


        var isHttpOrHttps = parsedUrl.Scheme == Uri.UriSchemeHttp || parsedUrl.Scheme == Uri.UriSchemeHttps;

        if (!isHttpOrHttps)
            throw new ArgumentException("A URL da fonte deve usar HTTP ou HTTPS.", nameof(url));


        return parsedUrl;
    }
}