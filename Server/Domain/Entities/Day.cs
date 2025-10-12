namespace Trivare.Domain.Entities;

public class Day
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }
    public ICollection<DayAttraction> DayAttractions { get; set; } = new List<DayAttraction>();
    public ICollection<File> Files { get; set; } = new List<File>();
}
