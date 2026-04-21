using EventManagerService.Properties;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Resources;

namespace EventManagerService.Infrastructure
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
#pragma warning disable CS8604 // Possible null reference argument.
                new ResourceManager(typeof(ErrorMessages)).GetString("UnhandledException"),
#pragma warning restore CS8604 // Possible null reference argument.
                context.Request.Method,
                context.Request.Path,
                context.Request.Headers["x-request-id"]);

            if (context.Response.HasStarted)
            {
                return;
            }
            var statusCode = MapStatusCode(ex);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var error = new ProblemDetails
            {
                Status = statusCode,
                Detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex) => ex switch
        {
            ValidationException ve => StatusCodes.Status400BadRequest,
            KeyNotFoundException ve => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError

        };
    }
}
