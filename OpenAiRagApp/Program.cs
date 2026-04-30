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
                if (ValidateArguments(args, out string action, out bool isSeedingNeeded))
                {
                    Console.WriteLine("Engine started...");
                    Console.WriteLine($"Mode: {action}, Seeding: {isSeedingNeeded}");

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

                    if (isSeedingNeeded)
                    {
                        //Only for seeding data to search index for demo purpose. Comment this when not needed.
                        var searchService = serviceProvider.GetRequiredService<ISemanticSearchService>();
                        await searchService.InitializeUploadAsync();
                    }

                    var app = serviceProvider.GetRequiredService<ConsoleApp>();
                    await app.Run(action);
                }
                else
                {
                    Console.WriteLine("Error: Invalid arguments provided.");
                    Console.WriteLine("Mode must be Chat, Search, or RAG. Seeding must be 1 or 0.");
                    Console.WriteLine("Usage: dotnet run <Chat|Search|RAG> <1|0>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static bool ValidateArguments(string[] args, out string action, out bool isSeedingNeeded)
        {
            action = args[0].ToLower() switch
            {
                "chat"   => "Chat",
                "search" => "Search",
                "rag"    => "RAG",
                _        => "Invalid"
            };
            isSeedingNeeded = args[1] switch
            {
                "1" => true,
                _   => false
            };
            return action != "Invalid";
        }
    }

    internal class ConsoleApp(IRagHandlerService _ragHandlerService,
                              IChatBotService _chatBotService,
                              ISemanticSearchService _semanticSearchService)
    {
        public async Task Run(string action)
        {
            Task task = action switch
            {
                //For free chat without RAG, which means the question will be directly sent to
                //Azure OpenAI without using the retrieved information from semantic search.
                "Chat"   => _chatBotService.FreeChatAsync(),

                //For free semantic search, which means you can test and see the retrieved information
                //from semantic search based on your query without sending it to Azure OpenAI.
                "Search" => _semanticSearchService.FreeSemanticSearchAsync(),

                //For complete RAG demo, which includes semantic search
                //and then using the retrieved information to answer the question.
                "RAG"    => _ragHandlerService.RunRagHandlerAsync(),

                _        => Task.Run(() => Console.WriteLine("Invalid mode. Please choose Chat, Search, or RAG."))
            };

            await task;
        }
    }


}