using Microsoft.Extensions.Options;

namespace GameMarketIntel.Collector.ExternalServices.Igdb;

public sealed class IgdbOptionsValidator : IValidateOptions<IgdbOptions>
{
    private const int MaximumPageSize = 500;
    private const int MaximumRetryAttempts = 5;
    private static readonly TimeSpan MinimumRequestInterval = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromMinutes(5);

    public ValidateOptionsResult Validate(string? name, IgdbOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.ClientId))
            failures.Add($"{IgdbOptions.SectionName}:ClientId is required.");

        if (string.IsNullOrWhiteSpace(options.ClientSecret))
            failures.Add($"{IgdbOptions.SectionName}:ClientSecret is required.");

        ValidateHttpsEndpoint(options.AuthenticationEndpoint, nameof(options.AuthenticationEndpoint), failures);
        ValidateApiBaseAddress(options.ApiBaseAddress, failures);

        if (options.PageSize is < 1 or > MaximumPageSize)
            failures.Add($"{IgdbOptions.SectionName}:PageSize must be between 1 and {MaximumPageSize}.");

        if (options.RequestInterval < MinimumRequestInterval)
            failures.Add($"{IgdbOptions.SectionName}:RequestInterval must be at least {MinimumRequestInterval.TotalMilliseconds} milliseconds.");

        if (options.RequestTimeout <= TimeSpan.Zero)
            failures.Add($"{IgdbOptions.SectionName}:RequestTimeout must be greater than zero.");

        if (options.MaxRetryAttempts is < 0 or > MaximumRetryAttempts)
            failures.Add($"{IgdbOptions.SectionName}:MaxRetryAttempts must be between 0 and {MaximumRetryAttempts}.");

        if (options.MaxRetryDelay <= TimeSpan.Zero || options.MaxRetryDelay > MaximumRetryDelay)
            failures.Add($"{IgdbOptions.SectionName}:MaxRetryDelay must be greater than zero and at most {MaximumRetryDelay.TotalMinutes} minutes.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateApiBaseAddress(Uri? endpoint, ICollection<string> failures)
    {
        ValidateHttpsEndpoint(endpoint, nameof(IgdbOptions.ApiBaseAddress), failures);

        // AbsolutePath cannot be read from a relative URI.
        if (endpoint?.IsAbsoluteUri != true)
            return;

        if (!endpoint.AbsolutePath.EndsWith("/", StringComparison.Ordinal))
            failures.Add($"{IgdbOptions.SectionName}:ApiBaseAddress path must end with '/'.");
    }

    private static void ValidateHttpsEndpoint(Uri? endpoint, string propertyName, ICollection<string> failures)
    {
        if (endpoint?.IsAbsoluteUri == true && endpoint.Scheme == Uri.UriSchemeHttps)
            return;

        failures.Add($"{IgdbOptions.SectionName}:{propertyName} must be an absolute HTTPS URI.");
    }
}
