namespace Trivare.Domain.Entities;

public class DayAttraction
{
    public Guid DayId { get; set; }
    public Day Day { get; set; } = null!;
    public Guid PlaceId { get; set; }
    public Place Place { get; set; } = null!;
    public int Order { get; set; }
    public bool IsVisited { get; set; }
}
