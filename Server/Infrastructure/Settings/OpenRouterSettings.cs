namespace Trivare.Infrastructure.Settings;

/// <summary>
/// Settings for OpenRouter AI service
/// </summary>
public class OpenRouterSettings
{
    /// <summary>
    /// OpenRouter API Key
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Model to use for place filtering (default: anthropic/claude-3.5-sonnet)
    /// </summary>
    public string Model { get; set; } = "anthropic/claude-3.5-sonnet";

    /// <summary>
    /// API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1";

    /// <summary>
    /// Number of places to return after AI filtering (default: 8)
    /// </summary>
    public int ResultCount { get; set; } = 8;
}

