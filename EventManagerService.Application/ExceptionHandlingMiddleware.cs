using EventManagerService.Domain.Exceptions;
using EventManagerService.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace EventManagerService.Application
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) 
            {
                await HandleException(context, ex);
            }
        }

        public async Task HandleException (HttpContext context, Exception ex)
        {
            _logger.LogError(
                ex,
                ErrorMessages.UnhandledException,
                context.Request.Method,
                context.Request.Path,
                context.Request.Headers["x-request-id"]);

            if (context.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            var error = new ProblemDetails
            {
                Type = GetProblemTypeUri(statusCode),
                Title = GetTitleForException(ex, statusCode),
                Status = statusCode,
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            error.Extensions["traceId"] = traceId;

            await context.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex) => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            FormatException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            NoAvailableSeatsException => StatusCodes.Status409Conflict,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string GetProblemTypeUri(int statusCode)
        {
            // Используем стандартный тип about:blank для распространённых кодов состояния
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "about:blank#bad-request",
                StatusCodes.Status404NotFound => "about:blank#not-found",
                StatusCodes.Status409Conflict => "about:blank#conflict",
                StatusCodes.Status403Forbidden => "about:blank#forbidden",
                _ => "about:blank#internal-server-error"
            };
        }

        private static string GetTitleForException(Exception ex, int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status403Forbidden => "Forbidden",
                _ => "Internal Server Error"
            };
        }
    }
}
