namespace Trivare.Domain.Entities;

public class Accommodation
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public string? Name { get; set; }
    public string? Address { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string? Notes { get; set; }
    public ICollection<File> Files { get; set; } = new List<File>();
}
