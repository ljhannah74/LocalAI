
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LocalAI.Models;
using LocalAI.Configuration;
using Microsoft.Extensions.Options;

namespace LocalAI.Services;

public sealed class OllamaTextAnalysisService(
    HttpClient httpClient, IOptions<OllamaOptions> options) : ITextAnalysisService
{
    public async Task<TextAnalysisResult> AnalyzeAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException(
                "Text cannot be empty.", nameof(text));

 
var request = new
{
    model = options.Value.Model,
    prompt = $"""
        Analyze the following text.

        Return the sentiment, named entities, and
        potential personally identifiable information.

        Sentiment must be one of:
        Positive, Negative, Neutral, Mixed.

        Entities and PII must be arrays of strings.
        Do not return objects inside these arrays.

        Text:
        {text}
        """,
    format = new
    {
        type = "object",
        properties = new
        {
            sentiment = new
            {
                type = "string",
                @enum = new[]
                {
                    "Positive", "Negative", "Neutral", "Mixed"
                }
            },
            entities = new
            {
                type = "array",
                items = new { type = "string" }
            },
            pii = new
            {
                type = "array",
                items = new { type = "string" }
            }
        },
        required = new[] { "sentiment", "entities", "pii" },
        additionalProperties = false
    },
    stream = false
};


        using var response = await httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<OllamaResponse>(
                cancellationToken);

        if (string.IsNullOrWhiteSpace(result?.Response))
            throw new InvalidOperationException(
                "Ollama returned an empty response.");

        var analysis = JsonSerializer.Deserialize<TextAnalysisResult>(
            result.Response);

        return analysis ?? throw new JsonException(
            "Could not deserialize the AI analysis.");
    }

    private sealed class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}
