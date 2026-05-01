using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAiRagApp.Services;

namespace OpenAiRagApp.Extensions;

public static class RegisterAppServices
{
    public static ServiceCollection RegisterServices(this ServiceCollection services)
    {
        var builder = new ConfigurationBuilder()
                                  .SetBasePath(Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", optional: false)
                                  .Build();

        services.AddOptions<AzureAiSettings>()
                                .Bind(builder.GetSection("AzureAiSettings"))
                                .ValidateDataAnnotations()
                                .ValidateOnStart();

        services.AddSingleton(sp =>
        {
            var s = sp.GetRequiredService<IOptions<AzureAiSettings>>().Value;
            return new SearchClient(new Uri(s.SearchEndpoint),
                                    s.SearchIndexName,
                                    new AzureKeyCredential(s.SearchApiKey));
        });

        services.AddSingleton(sp =>
        {
            var s = sp.GetRequiredService<IOptions<AzureAiSettings>>().Value;
            return new AzureOpenAIClient(new Uri(s.AzureOpenAiEndpoint),
                                         new AzureKeyCredential(s.AzureOpenAiApiKey));
        });

        services.AddTransient<ISemanticSearchService, SemanticSearchService>();
        services.AddTransient<IRagHandlerService, RagHandlerService>();
        services.AddTransient<IChatBotService, ChatBotService>();
        services.AddTransient<OpenAiApp>();

        return services;
    }
}

