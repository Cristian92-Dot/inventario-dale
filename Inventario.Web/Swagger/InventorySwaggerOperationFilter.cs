    using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Inventario.Web.Swagger;

public class InventorySwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.TryAdd("500", CreateJsonResponse("Error interno no controlado. La API no expone stack traces al cliente y siempre devuelve identificadores de diagnostico para trazabilidad tecnica."));

        var relativePath = "/" + context.ApiDescription.RelativePath?.TrimStart('/');
        var isAuthOperation = relativePath.StartsWith("/api/v1/Auth", StringComparison.OrdinalIgnoreCase);
        var isProtectedOperation = !isAuthOperation;
        var isMutation = context.ApiDescription.HttpMethod is "POST" or "PUT" or "DELETE";

        if (isProtectedOperation)
        {
            operation.Responses.TryAdd("401", CreateJsonResponse("Autenticacion requerida. Debe enviar un JWT valido usando el esquema Bearer."));
            operation.Responses.TryAdd("403", CreateJsonResponse("El usuario autenticado no cuenta con privilegios suficientes para ejecutar esta operacion."));
        }

        if (isMutation)
        {
            operation.Responses.TryAdd("422", CreateJsonResponse("La solicitud no cumple las validaciones de negocio o el payload no satisface las reglas aplicadas por la aplicacion."));
        }

        if (relativePath.Equals("/api/v1/Auth/refresh", StringComparison.OrdinalIgnoreCase) ||
            relativePath.Equals("/api/v1/Sales", StringComparison.OrdinalIgnoreCase))
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Llave unica generada por el cliente para volver idempotente la operacion. Si se reutiliza la misma llave con el mismo payload, la API devuelve exactamente la respuesta original. Si se reutiliza con un payload distinto, la API responde con HTTP 409.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Example = new OpenApiString("6fdf6ffc-ed77-94fa-407e-a7b86ed9e59d")
                }
            });

            operation.Responses.TryAdd("409", CreateJsonResponse("La misma llave de idempotencia ya fue utilizada con un payload diferente."));
        }

        if (isAuthOperation || relativePath.Equals("/api/v1/Sales", StringComparison.OrdinalIgnoreCase))
        {
            operation.Responses.TryAdd("429", CreateJsonResponse("La solicitud fue rechazada por la politica de rate limiting porque el endpoint supero el volumen permitido."));
        }

        operation.Description = AppendStandardBehavior(operation.Description, isProtectedOperation, relativePath);
    }

    private static OpenApiResponse CreateJsonResponse(string description)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
            }
        };
    }

    private static string AppendStandardBehavior(string? description, bool isProtectedOperation, string relativePath)
    {
        var notes = new List<string>();

        if (isProtectedOperation)
        {
            notes.Add("Requiere autenticacion JWT Bearer.");
        }

        if (relativePath.Equals("/api/v1/Sales", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add("Se ejecuta dentro de una transaccion de base de datos, valida stock de forma atomica y actualiza las alertas de reposicion una vez persistida la venta.");
        }

        if (relativePath.Equals("/api/v1/Auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            notes.Add("Rota el refresh token y revoca el anterior cuando la operacion finaliza correctamente.");
        }

        if (notes.Count == 0)
        {
            return description ?? string.Empty;
        }

        var appendix = string.Join(" ", notes);
        return string.IsNullOrWhiteSpace(description)
            ? appendix
            : $"{description} {appendix}";
    }
}
