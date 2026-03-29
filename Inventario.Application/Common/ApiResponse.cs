namespace Inventario.Application.Common;

/// <summary>
/// Envelope de respuesta estandar devuelto por la API.
/// </summary>
/// <typeparam name="T">Tipo del payload de negocio incluido en la respuesta.</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Indica si la operacion finalizo correctamente.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Resumen legible del resultado de la operacion.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Payload de negocio devuelto por el endpoint.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Coleccion de errores de validacion o negocio cuando la operacion falla.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Identificador de traza distribuida para diagnostico a lo largo del pipeline.
    /// </summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>
    /// Identificador de correlacion expuesto al cliente para soporte operativo.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Marca de tiempo UTC que indica cuando se genero la respuesta.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Operación completada correctamente.") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(IEnumerable<string> errors, string message = "La operación no pudo completarse.") =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors.ToArray()
        };
}
