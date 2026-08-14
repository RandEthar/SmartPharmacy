using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SmartPharmacy.PL.Middlewares
{
    /// <summary>
    /// Last line of defence for anything a controller did not catch. Without it a rejected file
    /// upload surfaced to the caller as a bare 500, and the exception was never recorded anywhere.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var (statusCode, title) = Map(exception);

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = httpContext.Request.Path
            };

            // Anything unmapped is a bug on our side, so the caller gets a generic message and
            // the details stay in the log - except in development, where they help while testing.
            if (statusCode == StatusCodes.Status500InternalServerError && _environment.IsDevelopment())
            {
                problem.Detail = exception.ToString();
            }
            else if (statusCode != StatusCodes.Status500InternalServerError)
            {
                problem.Detail = exception.Message;
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title) Map(Exception exception) => exception switch
        {
            // Thrown by FileService for a rejected upload and by CartService for an item that
            // cannot be sold - both are the caller's mistake, not a server failure.
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request."),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Request could not be completed."),

            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied."),
            TaskCanceledException or OperationCanceledException => (499, "Request cancelled."),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
        };
    }
}
