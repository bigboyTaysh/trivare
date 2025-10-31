using System.Text.Json.Serialization;

namespace Trivare.Domain.DTOs.OpenRouter;

/// <summary>
/// Response payload from OpenRouter chat completion API
/// </summary>
public record OpenRouterChatCompletionResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("choices")] List<Choice> Choices,
    [property: JsonPropertyName("created")] long? Created,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("usage")] Usage? Usage
);

/// <summary>
/// Choice in the chat completion response
/// </summary>
public record Choice(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("message")] MessageContent Message,
    [property: JsonPropertyName("finish_reason")] string? FinishReason
);

/// <summary>
/// Message content in the response
/// </summary>
public record MessageContent(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

/// <summary>
/// Token usage information
/// </summary>
public record Usage(
    [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
    [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
    [property: JsonPropertyName("total_tokens")] int TotalTokens
);

