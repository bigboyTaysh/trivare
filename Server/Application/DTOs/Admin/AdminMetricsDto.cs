namespace Trivare.Application.DTOs.Admin;

/// <summary>
/// Admin metrics data transfer object
/// Aggregated data from AuditLog, User, and Trip entities
/// Provides system usage statistics for specified time period
/// </summary>
public record AdminMetricsDto
{
    /// <summary>
    /// Time period information for the metrics
    /// </summary>
    public required MetricsPeriod Period { get; init; }

    /// <summary>
    /// Trip-related metrics
    /// </summary>
    public required TripsMetrics Trips { get; init; }

    /// <summary>
    /// User-related metrics
    /// </summary>
    public required UsersMetrics Users { get; init; }

    /// <summary>
    /// Search-related metrics from AuditLog
    /// </summary>
    public required SearchMetrics Searches { get; init; }

    /// <summary>
    /// AI usage metrics from AuditLog
    /// </summary>
    public required AiMetrics Ai { get; init; }
}

/// <summary>
/// Metrics period information
/// </summary>
public record MetricsPeriod
{
    /// <summary>
    /// Start of metrics period
    /// </summary>
    public required DateTime StartDate { get; init; }

    /// <summary>
    /// End of metrics period
    /// </summary>
    public required DateTime EndDate { get; init; }

    /// <summary>
    /// Number of days in period
    /// </summary>
    public required int Days { get; init; }
}

/// <summary>
/// Trip statistics derived from Trip entity
/// </summary>
public record TripsMetrics
{
    /// <summary>
    /// Total number of trips in system
    /// </summary>
    public required int Total { get; init; }

    /// <summary>
    /// Trips created in period
    /// </summary>
    public required int CreatedInPeriod { get; init; }

    /// <summary>
    /// Active trips (StartDate <= now <= EndDate)
    /// </summary>
    public required int Active { get; init; }
}

/// <summary>
/// User statistics derived from User entity
/// </summary>
public record UsersMetrics
{
    /// <summary>
    /// Total number of users in system
    /// </summary>
    public required int Total { get; init; }

    /// <summary>
    /// Users registered in period
    /// </summary>
    public required int RegisteredInPeriod { get; init; }
}

/// <summary>
/// Search statistics derived from AuditLog where EventType = "PlaceSearch"
/// </summary>
public record SearchMetrics
{
    /// <summary>
    /// Total searches performed in period
    /// </summary>
    public required int TotalSearches { get; init; }

    /// <summary>
    /// Average searches per user in period
    /// </summary>
    public required double AveragePerUser { get; init; }
}

/// <summary>
/// AI usage statistics derived from AuditLog AI-related events
/// </summary>
public record AiMetrics
{
    /// <summary>
    /// Total AI API calls in period
    /// </summary>
    public required int TotalCalls { get; init; }

    /// <summary>
    /// Average AI calls per search
    /// </summary>
    public required double AveragePerSearch { get; init; }
}
