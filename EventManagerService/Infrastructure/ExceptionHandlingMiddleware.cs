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
                new ResourceManager(typeof(ErrorMessages)).GetString("UnhandledException"),
                context.Request.Method,
                context.Request.Path,
                context.Request.Headers["x-request-id"]);

            if (context.Response.HasStarted)
            {
                return;
            }
            var stausCode = MapStatusCode(ex);

            context.Response.StatusCode = stausCode;
            context.Response.ContentType = "application/json";

            var error = new ProblemDetails
            {
                Status = stausCode,
                Detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(error);
        }

        private static int MapStatusCode(Exception ex) => ex switch
        {
            ValidationException ve => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError

        };
    }
}
