namespace Trivare.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string EventType { get; set; } = null!;
    public DateTime EventTimestamp { get; set; }
    public string? Details { get; set; }
}
