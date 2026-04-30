using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAiRagApp.Services;

namespace OpenAiRagApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: false)
                              .Build();

                var services = new ServiceCollection();

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
                services.AddTransient<ConsoleApp>();

                var serviceProvider = services.BuildServiceProvider();

                //Only for seeding data to search index for demo purpose. Comment this when not needed.
                //var searchService = serviceProvider.GetRequiredService<ISemanticSearchService>();
                //await searchService.InitializeUploadAsync();

                var app = serviceProvider.GetRequiredService<ConsoleApp>();
                await app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    public class ConsoleApp(
                            IRagHandlerService _ragHandlerService,
                            IChatBotService _chatBotService  
                           )
    {
        public async Task Run()
        {
            //For complete RAG demo, which includes semantic search
            //and then using the retrieved information to answer the question.
            await _ragHandlerService.RunRagHandlerAsync();

            //For free chat without RAG, which means the question will be directly sent to
            //Azure OpenAI without using the retrieved information from semantic search.
            //await _chatBotService.FreeChatAsync();
        }
    }
}