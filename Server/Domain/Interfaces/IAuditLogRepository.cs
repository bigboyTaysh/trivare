using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for AuditLog entity operations
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    /// <param name="auditLog">The audit log entry to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
