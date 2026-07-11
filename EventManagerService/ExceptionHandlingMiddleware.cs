using EventManagerService.Application.Exceptions;
using EventManagerService.Domain.Exceptions;
using EventManagerService.Shared.ErrorCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using EventManagerService.Properties;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventManagerService
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IStringLocalizer _localizer;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IStringLocalizerFactory localizerFactory)
        {
            _next = next;
            _logger = logger;
            // Создаём локализатор для ресурсов ErrorMessages
            var baseName = typeof(ErrorMessages).FullName!; // EventManagerService.Properties.ErrorMessages
            var asmName = typeof(ErrorMessages).Assembly.GetName().Name!;
            _localizer = localizerFactory.Create(baseName, asmName);
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

        public async Task HandleException(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing request {Method} {Path} RequestId:{RequestId}",
                context.Request.Method, context.Request.Path, context.Request.Headers["x-request-id"].ToString());

            if (context.Response.HasStarted)
            {
                return;
            }

            var statusCode = MapStatusCode(ex);
            var errorCode = ex is EventManagerService.Shared.Exceptions.AppException aex ? aex.ErrorCode : MapErrorCode(ex);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            // Формируем локализованное сообщение, если доступно
            string detail = ex.Message;
            try
            {
                if (ex is EventManagerService.Shared.Exceptions.AppException appEx)
                {
                    var localized = _localizer[appEx.ErrorCode];
                    if (!localized.ResourceNotFound)
                    {
                        // Подставляем параметры из Exception.Data, если они есть
                        var args = new List<object?>();
                        if (ex.Data.Contains("firstParamValue")) args.Add(ex.Data["firstParamValue"]);
                        if (ex.Data.Contains("secondParamValue")) args.Add(ex.Data["secondParamValue"]);
                        detail = args.Count > 0 ? string.Format(localized.Value, args.ToArray()) : localized.Value;
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки локализации и используем исходное сообщение
                detail = ex.Message;
            }

            var error = new ProblemDetails
            {
                Type = GetProblemTypeUri(statusCode),
                Title = GetTitleForException(ex, statusCode),
                Status = statusCode,
                Detail = detail,
                Instance = context.Request.Path
            };

            error.Extensions["traceId"] = traceId;
            error.Extensions["errorCode"] = errorCode;

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
            HightLoadException => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status500InternalServerError
        };

        private static string MapErrorCode(Exception ex) => ex switch
        {
            ValidationException => ErrorCodes.ValidationFailed,
            ArgumentException => ErrorCodes.ValidationFailed,
            FormatException => ErrorCodes.ValidationFailed,
            KeyNotFoundException => ErrorCodes.NotFound,
            NoAvailableSeatsException => ErrorCodes.Conflict,
            UnauthorizedAccessException => ErrorCodes.Forbidden,
            HightLoadException => ErrorCodes.HighLoad,
            _ => ErrorCodes.Unknown
        };

        private static string GetProblemTypeUri(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "about:blank#bad-request",
                StatusCodes.Status404NotFound => "about:blank#not-found",
                StatusCodes.Status409Conflict => "about:blank#conflict",
                StatusCodes.Status403Forbidden => "about:blank#forbidden",
                StatusCodes.Status429TooManyRequests => "about:blank#too-many-requests",
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
                StatusCodes.Status429TooManyRequests => "Too Many Requests",
                _ => "Internal Server Error"
            };
        }
    }
}
