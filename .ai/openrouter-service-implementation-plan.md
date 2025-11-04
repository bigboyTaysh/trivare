# OpenRouter Service Implementation Guide

This document provides a comprehensive plan for implementing the `OpenRouterService`. The service will act as a client for the OpenRouter.ai API, enabling Large Language Model (LLM) based chat completions with support for structured JSON responses.

## 1. Service Description

The `OpenRouterService` will be responsible for all communication with the OpenRouter API. It will be an `Infrastructure` layer component that implements an interface defined in the `Application` layer, adhering to Clean Architecture principles. Its primary function is to construct and send chat completion requests—including system messages, user messages, and model parameters—and to parse the LLM's response, with a strong focus on handling structured JSON data.

## 2. Constructor Description

The service will use constructor dependency injection to receive its dependencies.

```csharp
public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;

    public OpenRouterService(IHttpClientFactory httpClientFactory, IOptions<OpenRouterSettings> settings)
    {
        _httpClient = httpClientFactory.CreateClient("OpenRouterClient");
        _settings = settings.Value;

        // Configure HttpClient instance
        _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://trivare.app"); // Replace with actual app URL
        _httpClient.DefaultRequestHeaders.Add("X-Title", "Trivare"); // Replace with actual app name
    }
}
```

-   **`IHttpClientFactory`**: Used to create and manage `HttpClient` instances. A named client `OpenRouterClient` will be configured in `DependencyInjection.cs`.
-   **`IOptions<OpenRouterSettings>`**: Provides strongly-typed access to the service's configuration settings (API Key, Base URL).

## 3. Public Methods and Fields

The service will expose methods through the `IOpenRouterService` interface, which should be defined in the `Application` project.

### `Application/Interfaces/IOpenRouterService.cs`

```csharp
public interface IOpenRouterService
{
    Task<TResponse?> GetStructuredChatCompletionAsync<TResponse>(
        string userMessage,
        string systemMessage,
        string modelName,
        JsonSchema schema,
        CancellationToken cancellationToken = default) where TResponse : class;
}
```

### `GetStructuredChatCompletionAsync<TResponse>(...)`

This will be the primary method of the service.

-   **Description**: Sends a request to the OpenRouter API and expects a response that conforms to a specific JSON schema, which it will then deserialize into the generic type `TResponse`.
-   **Parameters**:
    -   `string userMessage`: The user's prompt.
    -   `string systemMessage`: Instructions for the LLM's behavior and response format.
    -   `string modelName`: The identifier for the LLM to be used (e.g., `openai/gpt-4o`).
    -   `JsonSchema schema`: An object representing the desired JSON schema for the response.
    -   `CancellationToken cancellationToken`: For cancelling the asynchronous operation.
-   **Returns**: A `Task` that resolves to an instance of `TResponse`, or `null` if the operation fails or returns no content.

## 4. Private Methods and Fields

-   **`private async Task<OpenRouterChatCompletionResponse?> SendRequestAsync(object requestBody, CancellationToken cancellationToken)`**: A helper method to handle the serialization of the request body, sending the `HttpRequestMessage`, and deserializing the response. This method will contain the core logic for the HTTP POST call.
-   **`private static StringContent SerializeRequest(object requestBody)`**: A helper to serialize the request payload into `StringContent` using `System.Text.Json` with appropriate serializer options (e.g., `JsonNamingPolicy.SnakeCaseLower`).

## 5. Error Handling

The service must implement robust error handling for API interactions.

1.  **Network Errors**: `HttpRequestException` will be thrown by `HttpClient` for network-level issues (e.g., DNS failure, connection refused). These should be caught and logged.
2.  **API Errors (4xx/5xx)**: If the API returns a non-success status code, the service should read the response body to get the error details from OpenRouter, log them, and throw a custom `OpenRouterApiException`.
3.  **Timeouts**: `TaskCanceledException` will be thrown if the request times out. This is handled by configuring a timeout on the `HttpClient` and using the `CancellationToken`.
4.  **Deserialization Errors**: `JsonException` can occur if the API response does not match the expected `TResponse` type or the JSON is malformed. The service should catch this, log the raw JSON response for debugging, and return `null` or re-throw as a custom exception.

## 6. Security Considerations

1.  **API Key Management**: The `ApiKey` must not be stored in source code.
    -   **Local Development**: Use the .NET Secret Manager: `dotnet user-secrets set "OpenRouterSettings:ApiKey" "YOUR_API_KEY"`
    -   **Production (Azure)**: Store the API key in Azure Key Vault and access it through Managed Identity.
2.  **Input Sanitization**: While the primary interaction is with the OpenRouter API, any user-provided content that is incorporated into prompts should be treated as untrusted. Be mindful of prompt injection, although this is a lower risk for server-to-server API calls.
3.  **Principle of Least Privilege**: The OpenRouter API key should be scoped with the minimum required permissions.

## 7. Step-by-Step Implementation Plan

### Step 1: Define Configuration Settings (`Infrastructure`)

Create a settings class to hold configuration from `appsettings.json`.

**File**: `Server/Infrastructure/Settings/OpenRouterSettings.cs`

```csharp
namespace Infrastructure.Settings
{
    public class OpenRouterSettings
    {
        public const string SectionName = "OpenRouterSettings";
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
}
```

### Step 2: Add Settings to Configuration Files

Update `appsettings.json` and its template. Store the actual key in user secrets.

**File**: `Server/Api/appsettings.Development.json`

```json
// ... existing settings
"OpenRouterSettings": {
  "ApiKey": "", // Leave empty, load from user secrets
  "BaseUrl": "https://openrouter.ai/api/v1"
}
```

**File**: `Server/Api/appsettings.Development.json.template`

```json
// ... existing settings
"OpenRouterSettings": {
  "ApiKey": "YOUR_OPENROUTER_API_KEY",
  "BaseUrl": "https://openrouter.ai/api/v1"
}
```

### Step 3: Define DTOs (`Application`)

Create Data Transfer Objects for the OpenRouter API request and response payloads.

**File**: `Server/Application/DTOs/OpenRouter/OpenRouterRequest.cs` (and related files)

```csharp
using System.Text.Json.Serialization;

namespace Application.DTOs.OpenRouter
{
    // Request DTOs
    public record OpenRouterChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
        [property: JsonPropertyName("response_format")] ResponseFormat ResponseFormat
    );

    public record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );

    public record ResponseFormat(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("json_schema")] JsonSchemaPayload JsonSchema
    );

    public record JsonSchemaPayload(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("strict")] bool Strict,
        [property: JsonPropertyName("schema")] JsonSchema Schema
    );
    
    // This can be a more complex object representing a JSON schema.
    // For simplicity, we can start with a dictionary.
    // A library like JsonSchema.Net could be used for more robust generation.
    public record JsonSchema(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("properties")] Dictionary<string, object> Properties,
        [property: JsonPropertyName("required")] List<string> Required
    );

    // Response DTOs
    public record OpenRouterChatCompletionResponse(
        [property: JsonPropertyName("choices")] List<Choice> Choices
    );

    public record Choice(
        [property: JsonPropertyName("message")] MessageContent Message
    );

    public record MessageContent(
        [property: JsonPropertyName("content")] string Content
    );
}
```

### Step 4: Implement the Service (`Infrastructure`)

Create the `OpenRouterService.cs` file with the logic to call the API.

**File**: `Server/Infrastructure/Services/OpenRouterService.cs` (Abridged)

```csharp
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Application.DTOs.OpenRouter;
using Application.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class OpenRouterService : IOpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly OpenRouterSettings _settings;
        private static readonly JsonSerializerOptions _serializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

        public OpenRouterService(IHttpClientFactory httpClientFactory, IOptions<OpenRouterSettings> settings)
        {
            // ... constructor logic as described above ...
        }

        public async Task<TResponse?> GetStructuredChatCompletionAsync<TResponse>(
            string userMessage, string systemMessage, string modelName, JsonSchema schema, CancellationToken cancellationToken) where TResponse : class
        {
            var request = new OpenRouterChatRequest(
                Model: modelName,
                Messages: new List<ChatMessage>
                {
                    new("system", systemMessage),
                    new("user", userMessage)
                },
                ResponseFormat: new ResponseFormat(
                    Type: "json_schema",
                    JsonSchema: new JsonSchemaPayload(
                        Name: $"get_{typeof(TResponse).Name.ToLower()}",
                        Strict: true,
                        Schema: schema
                    )
                )
            );

            var jsonRequest = JsonSerializer.Serialize(request, _serializerOptions);
            var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("/chat/completions", httpContent, cancellationToken);
                response.EnsureSuccessStatusCode(); // Throws on non-2xx responses

                var apiResponse = await response.Content.ReadFromJsonAsync<OpenRouterChatCompletionResponse>(cancellationToken: cancellationToken);
                var content = apiResponse?.Choices.FirstOrDefault()?.Message.Content;

                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<TResponse>(content);
            }
            catch (HttpRequestException ex)
            {
                // Log exception (ex.Message includes status code)
                // Optionally re-throw as custom exception
                return null;
            }
            catch (JsonException ex)
            {
                // Log deserialization error and the raw content
                return null;
            }
        }
    }
}
```

### Step 5: Configure Dependency Injection (`Infrastructure`)

Register the settings, HTTP client, and service in the DI container.

**File**: `Server/Infrastructure/DependencyInjection.cs`

```csharp
// ... using statements
using Infrastructure.Services;
using Infrastructure.Settings;

public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // ... other registrations

    // OpenRouter Service Configuration
    services.Configure<OpenRouterSettings>(configuration.GetSection(OpenRouterSettings.SectionName));

    services.AddHttpClient("OpenRouterClient", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(120); // Set a reasonable timeout for LLM requests
    });

    services.AddScoped<IOpenRouterService, OpenRouterService>();

    return services;
}
```

### Step 6: Define `IOpenRouterService` Interface (`Application`)

Create the interface file defined in the "Public Methods" section.

**File**: `Server/Application/Interfaces/IOpenRouterService.cs`

### Step 7: Usage Example (`Application/Services`)

Demonstrate how another service in the `Application` layer would use the `IOpenRouterService`.

```csharp
// In a service like 'PlacesService.cs'
public class PlacesService : IPlacesService
{
    private readonly IOpenRouterService _openRouterService;
    // ... other dependencies

    public PlacesService(IOpenRouterService openRouterService, /*...*/)
    {
        _openRouterService = openRouterService;
        // ...
    }

    public async Task<SomePlaceDetailsDto?> GetDetailsForPlaceAsync(string placeName)
    {
        var systemMessage = "You are a geography expert. Based on the user's input, provide details about the location in the specified JSON format.";
        var userMessage = $"Give me details about: {placeName}";

        var schema = new JsonSchema(
            Type: "object",
            Properties: new Dictionary<string, object>
            {
                ["country"] = new { type = "string", description = "The country the place is in." },
                ["population"] = new { type = "integer", description = "The estimated population." }
            },
            Required: new List<string> { "country", "population" }
        );

        var details = await _openRouterService.GetStructuredChatCompletionAsync<SomePlaceDetailsDto>(
            userMessage,
            systemMessage,
            "openai/gpt-4o",
            schema
        );

        return details;
    }
}

// Corresponding DTO
public record SomePlaceDetailsDto(string Country, int Population);
```
