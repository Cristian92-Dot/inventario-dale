using System.Diagnostics;
using System.Security.Claims;
using Inventario.Application.Common;
using Inventario.Application.Common.Exceptions;
using FluentValidation;
using Inventario.Domain.Entities;
using Inventario.Domain.Enums;
using Inventario.Infrastructure.Persistence;

namespace Inventario.Web.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            context.Items[HttpLoggingContextKeys.ExceptionType] = exception.GetType().Name;
            context.Items[HttpLoggingContextKeys.HasSecurityEvent] = true;
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

            try
            {
                var dbContext = context.RequestServices.GetService<ApplicationDbContext>();
                if (dbContext is not null)
                {
                    dbContext.ErrorLogs.Add(new ErrorLog
                    {
                        Message = exception.Message,
                        StackTrace = exception.StackTrace,
                        Source = exception.Source,
                        Path = context.Request.Path,
                        Method = context.Request.Method,
                        UserId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                        CorrelationId = context.TraceIdentifier,
                        CreatedAt = DateTime.UtcNow
                    });

                    dbContext.SecurityEvents.Add(new SecurityEvent
                    {
                        EventType = SecurityEventType.UnhandledException,
                        Severity = LogSeverity.Error,
                        Message = exception.Message,
                        UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                        UserName = context.User.Identity?.Name,
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.Request.Headers.UserAgent.ToString(),
                        Path = context.Request.Path,
                        Method = context.Request.Method,
                        CorrelationId = context.TraceIdentifier,
                        CreatedAt = DateTime.UtcNow
                    });

                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception logException)
            {
                _logger.LogWarning(logException, "Failed to persist error log.");
            }

            var (statusCode, errors, message) = exception switch
            {
                ValidationException validationException =>
                    (StatusCodes.Status400BadRequest,
                        validationException.Errors.Select(x => x.ErrorMessage).Distinct().ToArray(),
                        "Se detectaron errores de validación."),
                ForbiddenException =>
                    (StatusCodes.Status403Forbidden,
                        new[] { exception.Message },
                        "No tienes permisos para realizar esta acción."),
                UnauthorizedAccessException =>
                    (StatusCodes.Status401Unauthorized,
                        new[] { exception.Message },
                        "Debes iniciar sesión para continuar."),
                NotFoundException =>
                    (StatusCodes.Status404NotFound,
                        new[] { exception.Message },
                        "No se encontró el recurso solicitado."),
                ConflictException =>
                    (StatusCodes.Status409Conflict,
                        new[] { exception.Message },
                        "Se detectó un conflicto con la información enviada."),
                PdfGenerationException =>
                    (StatusCodes.Status500InternalServerError,
                        new[] { exception.Message },
                        "No fue posible generar el documento solicitado."),
                BusinessRuleException =>
                    (StatusCodes.Status400BadRequest,
                        new[] { exception.Message },
                        "No fue posible completar la operación por una regla de negocio."),
                InvalidOperationException =>
                    (StatusCodes.Status400BadRequest,
                        new[] { exception.Message },
                        "No fue posible procesar la solicitud."),
                _ =>
                    (StatusCodes.Status500InternalServerError,
                        new[] { "Ocurrió un error inesperado al procesar la solicitud." },
                        "No fue posible procesar la solicitud.")
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Fail(errors, message) with
            {
                TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
                CorrelationId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
