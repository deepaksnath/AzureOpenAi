using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;

namespace OpenAiRagApp.Services
{
    internal class SemanticSearchService(IOptions<AzureAiSettings> settings,
                                         AzureOpenAIClient client,
                                         SearchClient searchClient) : ISemanticSearchService
    {
        public async Task InitializeUploadAsync()
        {
            await UploadDocumentsAsync();
        }

        public async Task FreeSemanticSearchAsync()
        {
            Console.WriteLine("Semantic search Ready (type '0' to quit)");

            while (true)
            {
                Console.Write("\nUser: ");
                var query = Console.ReadLine();
                if (!string.IsNullOrEmpty(query))
                {
                    if (query == "0") break;

                    var queryEmbedding = await GetEmbeddingAsync(client, query);

                    var docs = await SearchDocumentsAsync(searchClient, queryEmbedding);

                    Console.WriteLine("\nAI: ");

                    foreach (string result in docs)
                    {
                        Console.WriteLine(result);
                    }
                }
            }

        }

        public async Task UploadDocumentsAsync()
        {

            var documents = new List<string>
            {
                "How to reset password",
                "Forgot login credentials",
                "Refund policy details",
                "How to change email address",
                "which is east direction",
                "How long is this path",
                "Distance from here to there",
            };

            if (documents.Any())
            {
                var docs = new List<object>();

                foreach (var text in documents)
                {
                    var emb = await GetEmbeddingAsync(client, text);

                    docs.Add(new
                    {
                        id = Guid.NewGuid().ToString(),
                        content = text,
                        vector = emb
                    });
                }
                await searchClient.UploadDocumentsAsync(docs);
            }
        }

        public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(AzureOpenAIClient client, string text)
        {
            EmbeddingClient embeddingClient = client.GetEmbeddingClient(settings.Value.EmbeddingModelDeployment);

            var result = await embeddingClient.GenerateEmbeddingsAsync(new List<string> { text });

            OpenAIEmbedding embedding = result.Value[0];

            return embedding.ToFloats();
        }

        public async Task<List<string>> SearchDocumentsAsync(SearchClient searchClient, ReadOnlyMemory<float> queryEmbedding)
        {
            var results = new List<string>();

            var options = new SearchOptions
            {
                Size = 3
            };

            options.Select.Add("content");

            options.VectorSearch = new()
            {
                Queries =
            {
                new VectorizedQuery(queryEmbedding)
                {
                    KNearestNeighborsCount = 5,
                    Fields = { "vector" }
                }
            }
            };

            var response = await searchClient.SearchAsync<SearchDocument>("*", options);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("content", out var content))
                {
                    var text = content?.ToString();
                    if (!string.IsNullOrEmpty(text))
                    {
                        results.Add(text);
                    }
                }
            }

            return results;
        }
    }
}
