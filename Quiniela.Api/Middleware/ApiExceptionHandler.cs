using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Quiniela.Api.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Data conflict"),
            _ => (0, null)
        };

        if (statusCode == 0)
        {
            return false;
        }

        logger.LogWarning(exception, "Request failed with {StatusCode}: {Title}", statusCode, title);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = title,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
