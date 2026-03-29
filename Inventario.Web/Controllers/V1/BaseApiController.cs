using System.Diagnostics;
using Asp.Versioning;
using Inventario.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Web.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ApiResponse<T> Success<T>(T data, string message = "Operación completada correctamente.") =>
        ApiResponse<T>.Ok(data, message) with
        {
            TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

    protected ApiResponse<object> Failure(IEnumerable<string> errors, string message) =>
        ApiResponse<object>.Fail(errors, message) with
        {
            TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };
}
