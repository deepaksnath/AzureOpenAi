using Azure.AI.OpenAI;
using Azure.Search.Documents;

namespace OpenAiRagApp.Services
{
    internal class RagHandlerService(ISemanticSearchService semanticSearchService,
                                     IChatBotService chatBotService,
                                     AzureOpenAIClient openAiClient,
                                     SearchClient searchClient) : IRagHandlerService
    {
        public async Task RunRagHandlerAsync()
        {
            Console.WriteLine("RAG Chatbot Ready (type '0' to quit)");

            while (true)
            {
                Console.Write("\nUser: ");
                var userQuery = Console.ReadLine();
                if (!string.IsNullOrEmpty(userQuery))
                {
                    if (userQuery.ToLower() == "0") break;

                    // Generate query embedding
                    var queryEmbedding = await semanticSearchService.GetEmbeddingAsync(openAiClient, userQuery);

                    // Vector search (Top-K)
                    var docs = await semanticSearchService.SearchDocumentsAsync(searchClient, queryEmbedding);

                    // Build context
                    var context = string.Join("\n", docs);

                    // Generate answer using LLM
                    var answer = await chatBotService.GetRagChatResponseAsync(context, userQuery);

                    Console.WriteLine($"\nAI: {answer}");
                }
            }
        }
    }
}

