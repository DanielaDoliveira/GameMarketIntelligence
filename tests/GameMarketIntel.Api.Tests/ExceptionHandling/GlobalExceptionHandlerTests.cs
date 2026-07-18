using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using GameMarketIntel.Api.ExceptionHandling;
using GameMarketIntel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace GameMarketIntel.Api.Tests.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var exception = new ValidationException(
        [
            new ValidationFailure( "Page",  "Page must be greater than or equal to 1."),

            new ValidationFailure( "ReleaseYear","Release year must be less than or equal to 2026.")
        ]);

        var (handler, httpContext) = CreateHandler( "/api/games?page=0&releaseYear=2027");

        // Act
        var handled = await handler.TryHandleAsync( httpContext,exception,CancellationToken.None);

        var response = await ReadResponseAsync(httpContext);

        // Assert
        handled.ShouldBeTrue();

        httpContext.Response.StatusCode.ShouldBe( StatusCodes.Status400BadRequest);

        response.RootElement
            .GetProperty("status")
            .GetInt32()
            .ShouldBe(StatusCodes.Status400BadRequest);

        response.RootElement
            .GetProperty("title")
            .GetString()
            .ShouldBe("Validation failed");

        response.RootElement
            .GetProperty("instance")
            .GetString()
            .ShouldBe("/api/games");

        var errors = response.RootElement.GetProperty("errors");

        errors.GetProperty("Page")[0]
            .GetString()
            .ShouldBe("Page must be greater than or equal to 1.");

        errors.GetProperty("ReleaseYear")[0]
            .GetString()
            .ShouldBe(
                "Release year must be less than or equal to 2026.");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnNotFound_WhenResourceDoesNotExist()
    {
        // Arrange
        var exception = new NotFoundException( "The requested game was not found.");

        var (handler, httpContext) = CreateHandler( "/api/games/70b3969b-b925-49a8-b250-ef49ff776e93");

        // Act
        var handled = await handler.TryHandleAsync( httpContext, exception, CancellationToken.None);

        var response = await ReadResponseAsync(httpContext);

        // Assert
        handled.ShouldBeTrue();

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);

        response.RootElement
            .GetProperty("title")
            .GetString()
            .ShouldBe("Resource not found");

        response.RootElement
            .GetProperty("detail")
            .GetString()
            .ShouldBe("The requested game was not found.");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnConflict_WhenResourceConflicts()
    {
        // Arrange
        var exception = new ConflictException(
            "A game with the same name already exists.");

        var (handler, httpContext) = CreateHandler("/api/games");

        // Act
        var handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            CancellationToken.None);

        var response = await ReadResponseAsync(httpContext);

        // Assert
        handled.ShouldBeTrue();

        httpContext.Response.StatusCode.ShouldBe(
            StatusCodes.Status409Conflict);

        response.RootElement
            .GetProperty("title")
            .GetString()
            .ShouldBe("Resource conflict");

        response.RootElement
            .GetProperty("detail")
            .GetString()
            .ShouldBe(
                "A game with the same name already exists.");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldReturnInternalServerError_WithoutExposingInternalDetails()
    {
        // Arrange
        const string internalMessage =
            "Database password and internal connection details.";

        var exception = new Exception(internalMessage);

        var (handler, httpContext) = CreateHandler("/api/games");

        // Act
        var handled = await handler.TryHandleAsync( httpContext,exception,CancellationToken.None);

        var response = await ReadResponseAsync(httpContext);

        // Assert
        handled.ShouldBeTrue();

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        response.RootElement
            .GetProperty("title")
            .GetString()
            .ShouldBe("An unexpected error occurred");

        response.RootElement
            .GetProperty("detail")
            .GetString()
            .ShouldBe(
                "An unexpected error occurred while processing the request.");

        response.RootElement
            .GetRawText()
            .ShouldNotContain(internalMessage);
    }

    private static (GlobalExceptionHandler Handler,DefaultHttpContext HttpContext)
        CreateHandler(string requestPath)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddProblemDetails();

        var serviceProvider = services.BuildServiceProvider();

        var problemDetailsService = serviceProvider.GetRequiredService<IProblemDetailsService>();

        var handler = new GlobalExceptionHandler(problemDetailsService,NullLogger<GlobalExceptionHandler>.Instance);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        httpContext.Request.Path = requestPath.Split('?')[0];

        httpContext.Response.Body = new MemoryStream();

        return (handler, httpContext);
    }

    private static async Task<JsonDocument> ReadResponseAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;

        return await JsonDocument.ParseAsync(httpContext.Response.Body);
    }
}