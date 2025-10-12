namespace Trivare.Domain.Entities;

public class Place
{
    public Guid Id { get; set; }
    public string? GooglePlaceId { get; set; }
    public string Name { get; set; } = null!;
    public string? FormattedAddress { get; set; }
    public string? Website { get; set; }
    public string? GoogleMapsLink { get; set; }
    public string? OpeningHoursText { get; set; }
    public bool IsManuallyAdded { get; set; }
    public ICollection<DayAttraction> DayAttractions { get; set; } = new List<DayAttraction>();
}
