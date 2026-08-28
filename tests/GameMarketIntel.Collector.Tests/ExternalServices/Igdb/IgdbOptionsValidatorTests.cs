using GameMarketIntel.Collector.ExternalServices.Igdb;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public sealed class IgdbOptionsValidatorTests
{
    private readonly IgdbOptionsValidator _validator = new();

    [Fact]
    public void Validate_ShouldSucceed_WhenOptionsAreValid()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCredentialsAreMissing()
    {
        // Arrange
        var options = new IgdbOptions
        {
            ClientId = " ",
            ClientSecret = string.Empty
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:ClientId is required.");
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:ClientSecret is required.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenEndpointsDoNotUseHttps()
    {
        // Arrange
        var options = new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            AuthenticationEndpoint = new Uri("http://localhost/oauth2/token"),
            ApiBaseAddress = new Uri("http://localhost/v4/")
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain(
            $"{IgdbOptions.SectionName}:AuthenticationEndpoint must be an absolute HTTPS URI.");
        result.Failures.ShouldContain(
            $"{IgdbOptions.SectionName}:ApiBaseAddress must be an absolute HTTPS URI.");
    }

    [Fact]
    public void Validate_ShouldReportEveryInvalidOperationalLimit()
    {
        // Arrange
        var options = new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            PageSize = 501,
            RequestInterval = TimeSpan.FromMilliseconds(249),
            RequestTimeout = TimeSpan.Zero,
            MaxRetryAttempts = 6,
            MaxRetryDelay = TimeSpan.FromMinutes(6)
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.Count().ShouldBe(5);
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:PageSize must be between 1 and 500.");
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:RequestInterval must be at least 250 milliseconds.");
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:RequestTimeout must be greater than zero.");
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:MaxRetryAttempts must be between 0 and 5.");
        result.Failures.ShouldContain($"{IgdbOptions.SectionName}:MaxRetryDelay must be greater than zero and at most 5 minutes.");
    }

    [Theory]
    [InlineData("https://api.igdb.com/v4")]
    [InlineData("https://example.invalid/custom/v4")]
    public void Validate_ShouldRejectApiBaseAddressWithoutTrailingSlash(string address)
    {
        // Arrange
        var options = CreateOptionsWithBaseAddress(new Uri(address));

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("Igdb:ApiBaseAddress path must end with '/'.");
        result.Failures.Count().ShouldBe(1);
        options.ApiBaseAddress.ShouldBe(new Uri(address));
    }

    [Theory]
    [InlineData("https://api.igdb.com/v4/", "https://api.igdb.com/v4/games")]
    [InlineData("https://example.invalid/custom/v4/", "https://example.invalid/custom/v4/games")]
    public void Validate_ShouldAcceptBaseAddressThatPreservesPath(string address, string expectedUrl)
    {
        // Arrange
        var options = CreateOptionsWithBaseAddress(new Uri(address));

        // Act
        var result = _validator.Validate(null, options);
        var requestUri = new Uri(options.ApiBaseAddress, "games");

        // Assert
        result.Succeeded.ShouldBeTrue();
        requestUri.ShouldBe(new Uri(expectedUrl));
    }

    [Fact]
    public void Validate_ShouldReportRelativeBaseAddressWithoutThrowing()
    {
        // Arrange
        var options = CreateOptionsWithBaseAddress(new Uri("v4/", UriKind.Relative));

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("Igdb:ApiBaseAddress must be an absolute HTTPS URI.");
        result.Failures.Count().ShouldBe(1);
    }

    [Fact]
    public void Validate_ShouldReportNullBaseAddressWithoutThrowing()
    {
        // Arrange
        var options = CreateOptionsWithBaseAddress(null!);

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("Igdb:ApiBaseAddress must be an absolute HTTPS URI.");
        result.Failures.Count().ShouldBe(1);
    }

    [Fact]
    public void Validate_ShouldNotRequireTrailingSlashOnAuthenticationEndpoint()
    {
        // Arrange
        var options = new IgdbOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            AuthenticationEndpoint = new Uri("https://example.invalid/oauth2/token")
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldCombineBaseAddressAndCredentialFailures()
    {
        // Arrange
        var options = new IgdbOptions
        {
            ClientId = string.Empty,
            ClientSecret = string.Empty,
            ApiBaseAddress = new Uri("https://api.igdb.com/v4")
        };

        // Act
        var result = _validator.Validate(null, options);

        // Assert
        result.Failed.ShouldBeTrue();
        result.Failures.Count().ShouldBe(3);
        result.Failures.ShouldContain("Igdb:ClientId is required.");
        result.Failures.ShouldContain("Igdb:ClientSecret is required.");
        result.Failures.ShouldContain("Igdb:ApiBaseAddress path must end with '/'.");
    }

    private static IgdbOptions CreateOptionsWithBaseAddress(Uri address) =>
        new()
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            ApiBaseAddress = address
        };

    private static IgdbOptions CreateValidOptions() =>
        new()
        {
            ClientId = "client-id",
            ClientSecret = "client-secret"
        };
}
