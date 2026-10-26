
using System.Text.Json.Serialization;

namespace LocalAI.Models;

public sealed class TextAnalysisResult
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; set; } = "";

    [JsonPropertyName("entities")]
    public List<string> Entities { get; set; } = [];

    [JsonPropertyName("pii")]
    public List<string> Pii { get; set; } = [];
}
