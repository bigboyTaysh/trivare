namespace Trivare.Domain.Entities;

public class Transport
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string? DepartureLocation { get; set; }
    public string? ArrivalLocation { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? Notes { get; set; }
    public ICollection<File> Files { get; set; } = new List<File>();
}
