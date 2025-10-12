namespace Trivare.Application.DTOs.Admin;

/// <summary>
/// Audit log entry data transfer object
/// Derived from AuditLog entity
/// </summary>
public record AuditLogDto
{
    /// <summary>
    /// Audit log identifier from AuditLog.Id
    /// </summary>
    public required long Id { get; init; }

    /// <summary>
    /// User who performed the action from AuditLog.UserId
    /// Null if user was deleted (GDPR) or system event
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// Type of event from AuditLog.EventType
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// Event timestamp from AuditLog.EventTimestamp
    /// </summary>
    public required DateTime EventTimestamp { get; init; }

    /// <summary>
    /// Additional event details (JSON) from AuditLog.Details
    /// </summary>
    public string? Details { get; init; }
}
