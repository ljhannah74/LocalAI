
using System.Net;
using System.Text.Json;
using LocalAI.Services;
using LocalAI.Configuration;
using Microsoft.Extensions.Options;

namespace LocalAI.Core.Tests;

public sealed class OllamaTextAnalysisServiceTests
{
    [Fact]
    public async Task AnalyzeAsync_ValidResponse_ReturnsAnalysis()
    {
        var analysis = new
        {
            sentiment = "Negative",
            entities = new[] { "Contoso" },
            pii = new[] { "555-123-4567" }
        };

        using var httpClient = CreateClient(
            HttpStatusCode.OK,
            analysis);

        var service = new OllamaTextAnalysisService(
    httpClient,
    Options.Create(new OllamaOptions()));

        var result = await service.AnalyzeAsync(
            "I am disappointed with Contoso.");

        Assert.Equal("Negative", result.Sentiment);
        Assert.Contains("Contoso", result.Entities);
        Assert.Contains("555-123-4567", result.Pii);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task AnalyzeAsync_EmptyText_ThrowsArgumentException(
        string text)
    {
        using var httpClient = new HttpClient();

        var service = new OllamaTextAnalysisService(
            httpClient,
            Options.Create(new OllamaOptions()));

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AnalyzeAsync(text));
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidJson_ThrowsJsonException()
    {
        using var httpClient = CreateClientWithRawResponse(
            HttpStatusCode.OK,
            "{invalid json}");

        var service = new OllamaTextAnalysisService(
            httpClient,
            Options.Create(new OllamaOptions()));

        await Assert.ThrowsAsync<JsonException>(
            () => service.AnalyzeAsync("Test text"));
    }

    [Fact]
    public async Task AnalyzeAsync_ServerError_ThrowsHttpRequestException()
    {
        using var httpClient = CreateClientWithRawResponse(
            HttpStatusCode.InternalServerError,
            "{}");

        var service = new OllamaTextAnalysisService(
            httpClient,
            Options.Create(new OllamaOptions()));

        await Assert.ThrowsAsync<HttpRequestException>(
            () => service.AnalyzeAsync("Test text"));
    }

    private static HttpClient CreateClient(
        HttpStatusCode statusCode,
        object analysis)
    {
        var analysisJson = JsonSerializer.Serialize(analysis);

        var ollamaResponse = JsonSerializer.Serialize(new
        {
            response = analysisJson
        });

        return CreateClientWithRawResponse(
            statusCode,
            ollamaResponse);
    }

    private static HttpClient CreateClientWithRawResponse(
        HttpStatusCode statusCode,
        string responseBody)
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    responseBody,
                    System.Text.Encoding.UTF8,
                    "application/json")
            });

        return new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };
    }
}
