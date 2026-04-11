using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Domain.Exceptions;

namespace Shared.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationException:
                problemDetails.Title = "Validation Error";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "One or more validation errors occurred.";
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray()
                    );
                break;
                
            case DomainException domainException:
                problemDetails.Title = "Domain Rule Violation";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = domainException.Message;
                break;
                
            case UnauthorizedAccessException unauthorizedAccessException:
                problemDetails.Title = "Unauthorized Access";
                problemDetails.Status = StatusCodes.Status403Forbidden;
                problemDetails.Detail = unauthorizedAccessException.Message;
                break;

            case KeyNotFoundException keyNotFoundException:
                problemDetails.Title = "Resource Not Found";
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Detail = keyNotFoundException.Message;
                break;
                
            default:
                problemDetails.Title = "An unexpected error occurred";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "Please contact support if the issue persists.";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}