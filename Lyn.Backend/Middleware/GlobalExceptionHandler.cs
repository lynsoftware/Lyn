using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lyn.Backend.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{ 
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1) Logg ALT (server-side)
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);


        // 2) Map til riktig statuskode + ProblemDetails
        var (status, title, detail) = exception switch
        {
            ArgumentException argEx => (StatusCodes.Status400BadRequest, "Bad Request", argEx.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", exception.Message),
            UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation Error", ex.Message),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database Conflict", 
                "A conflict occurred while saving data. The resource may have been modified or deleted."),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };


        var problem = new ProblemDetails
        {
            Type = GetProblemTypeUri(status),
            Status = status,
            Title  = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };


        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
      
        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true; // vi håndterte feilen
    }


    private static string GetProblemTypeUri(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        401 => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
        403 => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
        404 => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        500 => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        _ => "https://tools.ietf.org/html/rfc9110"
    };
}

