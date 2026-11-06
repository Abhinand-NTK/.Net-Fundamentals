using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PersonalDetailsApi.Common.Exceptions;

namespace PersonalDetailsApi.Common.Errors;

/// <summary>
/// Single place that converts exceptions into RFC 7807 <see cref="ProblemDetails"/> responses.
/// Expected errors (<see cref="AppException"/>) keep their message; anything else becomes a generic 500
/// so internal details never leak to clients.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            AppException app => (app.StatusCode, app.Title, app.Message),
            OperationCanceledException => (StatusCodes.Status499ClientClosedRequest, "Request cancelled", null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", (string?)null),
        };

        if (status >= 500)
            logger.LogError(exception, "Unhandled exception for {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        else
            logger.LogWarning("{Title}: {Detail}", title, detail);

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
            },
        });
    }
}
