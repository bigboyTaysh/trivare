using System.Text.Json.Serialization;

namespace Trivare.Domain.DTOs.OpenRouter;

/// <summary>
/// Request payload for OpenRouter chat completion API
/// </summary>
public record OpenRouterChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
    [property: JsonPropertyName("response_format")] ResponseFormat ResponseFormat
);

/// <summary>
/// Chat message with role and content
/// </summary>
public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

/// <summary>
/// Response format configuration for structured JSON output
/// </summary>
public record ResponseFormat(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("json_schema")] JsonSchemaPayload? JsonSchema
);

/// <summary>
/// JSON schema payload wrapper
/// </summary>
public record JsonSchemaPayload(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("strict")] bool Strict,
    [property: JsonPropertyName("schema")] JsonSchema Schema
);

/// <summary>
/// JSON schema definition for structured responses
/// </summary>
public record JsonSchema(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("properties")] Dictionary<string, object> Properties,
    [property: JsonPropertyName("required")] List<string> Required,
    [property: JsonPropertyName("additionalProperties")] bool AdditionalProperties = false
);

