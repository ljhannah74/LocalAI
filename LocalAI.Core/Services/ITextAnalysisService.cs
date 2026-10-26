
using LocalAI.Models;

namespace LocalAI.Services;

public interface ITextAnalysisService
{
    Task<TextAnalysisResult> AnalyzeAsync(
        string text,
        CancellationToken cancellationToken = default);
}
