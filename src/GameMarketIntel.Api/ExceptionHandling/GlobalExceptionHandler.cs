using FluentValidation;
using GameMarketIntel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GameMarketIntel.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler( IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(  HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblemDetails(
                    httpContext,
                    validationException),

            NotFoundException notFoundException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Resource not found",
                    notFoundException.Message),

            ConflictException conflictException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status409Conflict,
                    "Resource conflict",
                    conflictException.Message),

            _ =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred",
                    "An unexpected error occurred while processing the request.")
        };

        LogException(exception, problemDetails.Status);

        httpContext.Response.StatusCode =problemDetails.Status  ?? StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
    }

    private static ProblemDetails CreateProblemDetails( HttpContext httpContext,int status, string title, string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails( HttpContext httpContext,ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred.",
            Instance = httpContext.Request.Path
        };
    }

    private void LogException(Exception exception,int? statusCode)
    {
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(  exception, "An unexpected exception occurred while processing the request.");
            return;
        }

        _logger.LogWarning( exception, "A handled exception occurred while processing the request. Status code: {StatusCode}",statusCode);
    }
}