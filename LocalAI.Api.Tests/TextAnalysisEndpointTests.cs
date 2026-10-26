
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LocalAI.Models;

namespace LocalAI.Api.Tests;

public sealed class TextAnalysisEndpointTests
{
    [Fact]
    public async Task GetRoot_ReturnsOk()
    {
        using var factory = new LocalAIApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "LocalAI API is running!",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AnalyzeText_ValidRequest_ReturnsAnalysis()
    {
        using var factory = new LocalAIApiFactory();

        factory.FakeService.OnAnalyze = text =>
        {
            Assert.Equal("Contoso disappointed me.", text);

            return new TextAnalysisResult
            {
                Sentiment = "Negative",
                Entities = ["Contoso"],
                Pii = []
            };
        };

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text = "Contoso disappointed me." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<TextAnalysisResult>();

        Assert.NotNull(result);
        Assert.Equal("Negative", result.Sentiment);
        Assert.Contains("Contoso", result.Entities);
        Assert.Empty(result.Pii);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task AnalyzeText_EmptyText_ReturnsBadRequest(
        string text)
    {
        using var factory = new LocalAIApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task AnalyzeText_OllamaUnavailable_Returns503()
    {
        using var factory = new LocalAIApiFactory();

        factory.FakeService.ExceptionToThrow =
            new HttpRequestException(
                "Connection refused.");

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text = "Test text" });

        Assert.Equal(
            HttpStatusCode.ServiceUnavailable,
            response.StatusCode);
    }

    [Fact]
    public async Task AnalyzeText_InvalidAiResponse_Returns502()
    {
        using var factory = new LocalAIApiFactory();

        factory.FakeService.ExceptionToThrow =
            new JsonException("Invalid AI response.");

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text = "Test text" });

        Assert.Equal(
            HttpStatusCode.BadGateway,
            response.StatusCode);
    }

    [Fact]
    public async Task AnalyzeText_ModelNotFound_Returns502()
    {
        using var factory = new LocalAIApiFactory();

        factory.FakeService.ExceptionToThrow =
            new HttpRequestException(
                "Model not found.",
                null,
                HttpStatusCode.NotFound);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text = "Test text" });

        Assert.Equal(
            HttpStatusCode.BadGateway,
            response.StatusCode);
    }

    [Fact]
    public async Task AnalyzeText_Timeout_Returns504()
    {
        using var factory = new LocalAIApiFactory();

        factory.FakeService.ExceptionToThrow =
            new TimeoutException("Request timed out.");

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/textanalysis",
            new { text = "Test text" });

        Assert.Equal(
            HttpStatusCode.GatewayTimeout,
            response.StatusCode);
    }
}
