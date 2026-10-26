
using LocalAI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using LocalAI.Configuration;
using Microsoft.Extensions.Options;

var services = new ServiceCollection();


services.Configure<OllamaOptions>(options =>
{
    options.BaseUrl = "http://localhost:11434";
    options.Model = "llama3.2:1b";
    options.TimeoutSeconds = 180;
});

services.AddHttpClient<
    ITextAnalysisService,
    OllamaTextAnalysisService>((provider, client) =>
{
    var options = provider
        .GetRequiredService<IOptions<OllamaOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(
        options.TimeoutSeconds);
});


using var provider = services.BuildServiceProvider();

var analyzer = provider.GetRequiredService<ITextAnalysisService>();

Console.WriteLine("Local AI Text Analyzer");
Console.WriteLine("----------------------");
Console.Write("Enter text to analyze: ");

string? text = Console.ReadLine();

if (string.IsNullOrWhiteSpace(text))
{
    Console.WriteLine("No text entered.");
    return;
}

try
{
    var result = await analyzer.AnalyzeAsync(text);

    Console.WriteLine();
    Console.WriteLine($"Sentiment: {result.Sentiment}");

    Console.WriteLine("Named Entities:");
    foreach (var entity in result.Entities)
        Console.WriteLine($"  - {entity}");

    Console.WriteLine("Potential PII:");
    foreach (var item in result.Pii)
        Console.WriteLine($"  - {item}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Ollama request failed: {ex.Message}");
}
catch (JsonException ex)
{
    Console.WriteLine($"Invalid AI response: {ex.Message}");
}
catch (TaskCanceledException)
{
    Console.WriteLine("The AI request timed out.");
}
catch (Exception ex)
{
    Console.WriteLine($"Analysis failed: {ex.Message}");
}
