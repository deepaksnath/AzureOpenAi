using Microsoft.Extensions.DependencyInjection;
using OpenAiRagApp.Extensions;

namespace OpenAiRagApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                if (ArgumentsValidator.ValidateArguments(args, out string action, out bool isSeedingNeeded))
                {
                    Console.WriteLine("Engine started...");
                    Console.WriteLine($"Mode: {action}, Seeding: {isSeedingNeeded}");
                    
                    var services = new ServiceCollection();
                    services.RegisterServices();

                    var serviceProvider = services.BuildServiceProvider();
                    var app = serviceProvider.GetRequiredService<OpenAiApp>();
                    await app.Run(action, isSeedingNeeded);
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
    }
}