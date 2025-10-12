namespace Trivare.Domain.Entities;

public class Trip
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Destination { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public Transport? Transport { get; set; }
    public Accommodation? Accommodation { get; set; }
    public ICollection<Day> Days { get; set; } = new List<Day>();
    public ICollection<File> Files { get; set; } = new List<File>();
}
