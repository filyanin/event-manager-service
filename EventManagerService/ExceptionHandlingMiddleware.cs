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
            // базовое им€ ресурса ErrorMessages
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

            // ‘ормируем детализацию ошибки: если в ресурсах нет строки Ч возвращаем код ошибки.
            string detail = GetLocalizedDetailOrCode(ex, errorCode, context);

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

        private string GetLocalizedDetailOrCode(Exception ex, string errorCode, HttpContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(errorCode))
                    return errorCode ?? string.Empty;

                var localized = _localizer[errorCode];
                if (localized.ResourceNotFound)
                {
                    // если нет текста в ресурсах Ч возвращаем сам код ошибки
                    return errorCode;
                }

                var template = localized.Value ?? string.Empty;

                // »щем именованные плейсхолдеры {name}
                var matches = System.Text.RegularExpressions.Regex.Matches(template, "\\{(?<name>[^}]+)\\}");
                if (matches.Count == 0)
                {
                    return template;
                }

                // —обираем возможные значени€ дл€ подстановки
                var replacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                replacements["Method"] = context.Request.Method;
                replacements["Path"] = context.Request.Path.ToString();
                replacements["RequestId"] = context.Request.Headers["x-request-id"].ToString();

                if (ex?.Data != null)
                {
                    foreach (var key in ex.Data.Keys)
                    {
                        try
                        {
                            var keyStr = key?.ToString() ?? string.Empty;
                            var val = ex.Data[key]?.ToString() ?? string.Empty;
                            if (!string.IsNullOrEmpty(keyStr))
                                replacements[keyStr] = val;
                        }
                        catch
                        {
                            // пропускаем
                        }
                    }
                }

                // ”бедимс€, что дл€ всех плейсхолдеров есть значение; если нет Ч возвращаем шаблон без изменений
                foreach (System.Text.RegularExpressions.Match m in matches)
                {
                    var name = m.Groups["name"].Value;
                    if (!replacements.ContainsKey(name))
                    {
                        return template;
                    }
                }

                // ¬ыполн€ем подстановку
                var result = template;
                foreach (var kv in replacements)
                {
                    result = result.Replace("{" + kv.Key + "}", kv.Value);
                }

                return result;
            }
            catch
            {
                // в случае ошибок в локализации безопасно возвращаем код ошибки
                return errorCode;
            }
        }

        private static int MapStatusCode(Exception ex) => ex switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            FormatException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            NoAvailableSeatsException => StatusCodes.Status409Conflict,
            ActiveBookingsLimitException => StatusCodes.Status409Conflict,
            PastEventBookingException => StatusCodes.Status400BadRequest,
            UnauthorizedBookingCancellationException => StatusCodes.Status403Forbidden,
            UnauthorizedOperationException => StatusCodes.Status403Forbidden,
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
            ActiveBookingsLimitException => ErrorCodes.Conflict,
            PastEventBookingException => ErrorCodes.PastEventBookingError,
            UnauthorizedBookingCancellationException => ErrorCodes.UnauthorizedBookingCancellationError,
            UnauthorizedOperationException => ErrorCodes.UnauthorizedOperationError,
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
