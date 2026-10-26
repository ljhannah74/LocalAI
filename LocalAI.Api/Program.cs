
using LocalAI.Services;
using LocalAI.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<OllamaOptions>(
    builder.Configuration.GetSection(
        OllamaOptions.SectionName));

builder.Services.AddHttpClient<
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


var app = builder.Build();

app.MapGet("/", () => "LocalAI API is running!");

app.MapPost("/api/textanalysis", async (
    AnalyzeTextRequest request,
    ITextAnalysisService analyzer,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
    {
        return Results.BadRequest(new
        {
            error = "Text cannot be empty."
        });
    }

    try
    {
        var result = await analyzer.AnalyzeAsync(
            request.Text,
            cancellationToken);

        return Results.Ok(result);
    }
    catch (HttpRequestException ex)
    {
        logger.LogError(ex, "Ollama HTTP request failed.");

        var status = ex.StatusCode ==
            System.Net.HttpStatusCode.NotFound
            ? StatusCodes.Status502BadGateway
            : ex.StatusCode is null
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status502BadGateway;

        return Results.Json(
            new { error = "The AI service could not complete the request." },
            statusCode: status);
    }
    catch (JsonException ex)
    {
        logger.LogError(ex, "Invalid AI response.");

        return Results.Json(
            new { error = "The AI returned an invalid response." },
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (TimeoutException ex)
    {
        logger.LogError(ex, "AI request timed out.");

        return Results.Json(
            new { error = "The AI request timed out." },
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (OperationCanceledException ex)
        when (!cancellationToken.IsCancellationRequested)
    {
        logger.LogError(ex, "AI request timed out.");

        return Results.Json(
            new { error = "The AI request timed out." },
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError(ex, "Invalid AI response.");

        return Results.Json(
            new { error = "The AI returned an invalid response." },
            statusCode: StatusCodes.Status502BadGateway);
    }
});

app.Run();

public sealed record AnalyzeTextRequest(string Text);
public partial class Program { }