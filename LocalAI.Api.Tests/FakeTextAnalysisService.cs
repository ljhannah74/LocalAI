
using LocalAI.Models;
using LocalAI.Services;

namespace LocalAI.Api.Tests;

public sealed class FakeTextAnalysisService : ITextAnalysisService
{
    public Func<string, TextAnalysisResult>? OnAnalyze { get; set; }

    public Exception? ExceptionToThrow { get; set; }

    public Task<TextAnalysisResult> AnalyzeAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        var result = OnAnalyze?.Invoke(text)
            ?? new TextAnalysisResult
            {
                Sentiment = "Negative",
                Entities = ["Contoso"],
                Pii = []
            };

        return Task.FromResult(result);
    }
}
