namespace Trivare.Domain.Entities;

public class File
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSize { get; set; }
    public string FileType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public Guid? TripId { get; set; }
    public Trip? Trip { get; set; }
    public Guid? TransportId { get; set; }
    public Transport? Transport { get; set; }
    public Guid? AccommodationId { get; set; }
    public Accommodation? Accommodation { get; set; }
    public Guid? DayId { get; set; }
    public Day? Day { get; set; }
}
