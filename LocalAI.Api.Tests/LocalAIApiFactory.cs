
using LocalAI.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LocalAI.Api.Tests;

public sealed class LocalAIApiFactory : WebApplicationFactory<Program>
{
    public FakeTextAnalysisService FakeService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITextAnalysisService>();

            // Remove the typed HttpClient registration so that
            // the fake is always used during integration tests.
            services.AddSingleton<ITextAnalysisService>(FakeService);
        });
    }
}
