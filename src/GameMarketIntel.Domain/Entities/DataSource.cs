using System.Text.RegularExpressions;
using GameMarketIntel.Domain.ValueObjects;

namespace GameMarketIntel.Domain.Entities;

public sealed partial class DataSource
{
    public const int MaxCodeLength = 50;

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Url { get; private set; } = string.Empty;

    public string? LicenseNotes { get; private set; }

    public bool AttributionRequired { get; private set; }

    public SourceReliability Reliability { get; private set; } = null!;

    private DataSource()
    {
    }

    public DataSource(
        string code,
        string name,
        string url,
        SourceReliability reliability,
        bool attributionRequired,
        string? licenseNotes = null)
    {
        Code = NormalizeAndValidateCode(code);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "The data source name is required.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException(
                "The data source URL is required.",
                nameof(url));
        }

        var parsedUrl = ValidateAndParseUrl(url);

        ArgumentNullException.ThrowIfNull(reliability);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Url = parsedUrl.ToString();
        Reliability = reliability;
        AttributionRequired = attributionRequired;
        LicenseNotes = string.IsNullOrWhiteSpace(licenseNotes)
            ? null
            : licenseNotes.Trim();
    }

    private static string NormalizeAndValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "The data source code is required.",
                nameof(code));
        }

        var normalizedCode = code.Trim().ToLowerInvariant();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new ArgumentException(
                $"The data source code cannot exceed {MaxCodeLength} characters.",
                nameof(code));
        }

        if (!CodePattern().IsMatch(normalizedCode))
        {
            throw new ArgumentException(
                "The data source code must contain only lowercase letters, numbers, and single hyphens between segments.",
                nameof(code));
        }

        return normalizedCode;
    }

    private static Uri ValidateAndParseUrl(string url)
    {
        var isValidAbsoluteUrl = Uri.TryCreate(
            url.Trim(),
            UriKind.Absolute,
            out var parsedUrl);

        if (!isValidAbsoluteUrl || parsedUrl is null)
        {
            throw new ArgumentException(
                "The data source URL must be a valid absolute URL.",
                nameof(url));
        }

        var isHttpOrHttps =
            parsedUrl.Scheme == Uri.UriSchemeHttp ||
            parsedUrl.Scheme == Uri.UriSchemeHttps;

        if (!isHttpOrHttps)
        {
            throw new ArgumentException(
                "The data source URL must use HTTP or HTTPS.",
                nameof(url));
        }

        return parsedUrl;
    }

    [GeneratedRegex(
        "^[a-z0-9]+(?:-[a-z0-9]+)*$",
        RegexOptions.CultureInvariant)]
    private static partial Regex CodePattern();
}