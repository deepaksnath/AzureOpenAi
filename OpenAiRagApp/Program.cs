using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAiRagApp.Services;
using static OpenAiRagApp.Constants;

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
                    services.AddTransient<OpenAiApp>();

                    var serviceProvider = services.BuildServiceProvider();

                    if (isSeedingNeeded)
                    {
                        //Only for seeding data to search index for demo purpose.
                        var searchService = serviceProvider.GetRequiredService<ISemanticSearchService>();
                        await searchService.InitializeUploadAsync();
                    }

                    var app = serviceProvider.GetRequiredService<OpenAiApp>();
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
            string firstArg = args?.Length > 0 ? args[0] : string.Empty;

            if (Enum.TryParse(firstArg, true, out Mode mode))
            {
                action = mode.ToString();
            }
            else
            {
                action = Mode.INVALID.ToString();
            }

            isSeedingNeeded = args?.Length > 1 && args[1] == "1";

            return action != Mode.INVALID.ToString();
        }
    }
}