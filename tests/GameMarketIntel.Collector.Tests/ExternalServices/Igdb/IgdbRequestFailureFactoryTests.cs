using System.Net;
using System.Net.Http.Headers;
using GameMarketIntel.Collector.ExternalServices.Igdb;
using Shouldly;

namespace GameMarketIntel.Collector.Tests.ExternalServices.Igdb;

public class IgdbRequestFailureFactoryTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public void CreateException_ShouldPreserveStatusCode(HttpStatusCode statusCode)
    {
        // Arrange
        using var response = new HttpResponseMessage(statusCode);

        // Act
        var exception = IgdbRequestFailureFactory.CreateException(response);

        // Assert
        exception.StatusCode.ShouldBe((HttpStatusCode?)statusCode);
        exception.Message.ShouldBe($"IGDB request failed with status code {(int)statusCode}.");
        exception.InnerException.ShouldBeNull();
    }
    [Fact]
    public void CreateException_ShouldExcludeRemoteTextAndRequestDetails()
    {
        // Arrange
        const string sensitiveText = "synthetic-sensitive-value";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://example.invalid/games?secret={sensitiveText}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sensitiveText);
        request.Headers.Add("Client-ID", sensitiveText);
        request.Content = new StringContent(sensitiveText);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(sensitiveText),
            ReasonPhrase = sensitiveText,
            RequestMessage = request
        };
        response.Headers.Add("X-Diagnostic", sensitiveText);

        // Act
        var exception = IgdbRequestFailureFactory.CreateException(response);

        // Assert
        exception.Message.ShouldBe("IGDB request failed with status code 400.");
        exception.ToString().ShouldNotContain(sensitiveText);
        exception.ToString().ShouldNotContain("example.invalid");
        exception.Data.Count.ShouldBe(0);
        exception.InnerException.ShouldBeNull();
    }
    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.NoContent)]
    public void CreateException_ShouldRejectSuccessfulResponse(HttpStatusCode statusCode)
    {
        // Arrange
        using var response = new HttpResponseMessage(statusCode);

        // Act
        Action action = () => IgdbRequestFailureFactory.CreateException(response);

        // Assert
        var exception = Should.Throw<ArgumentException>(action);
        exception.ParamName.ShouldBe("response");
    }

    [Fact]
    public void CreateException_ShouldRejectNullResponse()
    {
        // Arrange
        HttpResponseMessage response = null!;

        // Act
        Action action = () => IgdbRequestFailureFactory.CreateException(response);

        // Assert
        var exception = Should.Throw<ArgumentNullException>(action);
        exception.ParamName.ShouldBe("response");
    }
}