
using System.Net;

namespace LocalAI.Core.Tests;

public sealed class FakeHttpMessageHandler(
    Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(responseFactory(request));
    }
}
