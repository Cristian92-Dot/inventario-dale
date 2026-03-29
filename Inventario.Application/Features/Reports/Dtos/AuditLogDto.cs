namespace Inventario.Application.Features.Reports.Dtos;

/// <summary>
/// Proyeccion administrativa que representa una entrada reciente de auditoria funcional.
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// Identificador de la entrada de auditoria.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Accion de negocio capturada por el subsistema de auditoria.
    /// </summary>
    public string ActionType { get; init; } = string.Empty;

    /// <summary>
    /// Entidad de dominio o recurso tecnico afectado por la accion.
    /// </summary>
    public string EntityName { get; init; } = string.Empty;

    /// <summary>
    /// Usuario responsable de la accion auditada, cuando exista.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    /// Identificador de correlacion asociado a la solicitud original.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Fecha y hora UTC en que se persistio el evento de auditoria.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
