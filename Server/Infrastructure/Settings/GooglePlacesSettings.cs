namespace Trivare.Infrastructure.Settings;

/// <summary>
/// Settings for Google Places API
/// </summary>
public class GooglePlacesSettings
{
    /// <summary>
    /// Google Places API Key
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Maximum number of results to fetch from Google Places API (default: 20)
    /// </summary>
    public int MaxResults { get; set; } = 20;

    /// <summary>
    /// Search radius in meters (default: 5000)
    /// </summary>
    public int SearchRadiusMeters { get; set; } = 5000;
}

