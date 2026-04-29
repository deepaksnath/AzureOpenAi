using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Embeddings;

namespace OpenAiRagApp.Services
{
    internal class SemanticSearchService(AzureAiSettings settings)
    {
        public async Task RunSearch()
        {
            //await UploadDocuments(settings);

            var client = new AzureOpenAIClient(
                         new Uri(settings.EmbEndpoint),
                         new AzureKeyCredential(settings.EmbApiKey));

            var searchClient = new SearchClient(
                               new Uri(settings.SearchEndpoint),
                               settings.searchIndexName,
                               new AzureKeyCredential(settings.SearchApiKey));

            while (true)
            {
                Console.Write("\nUser: ");
                var query = Console.ReadLine();

                if (query == "0") break;

                var queryEmbedding = await GetEmbedding(client, query);

                var options = new SearchOptions
                {
                    Size = 2,
                    Select = { "id", "content" }
                };
                options.VectorSearch = new()
                {
                    Queries =
                                {
                                    new VectorizedQuery(queryEmbedding)
                                    {
                                        KNearestNeighborsCount = 2,
                                        Fields =  { "vector" }
                                    }
                                }
                };

                var response = await searchClient.SearchAsync<SearchDocument>("*", options);

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    Console.WriteLine(result.Document["content"]);
                }
            }

        }

        public async Task UploadDocuments()
        {

            var documents = new List<string>
            {
                //"How to reset password",
                //"Forgot login credentials",
                //"Refund policy details",
                //"How to change email address",
                //"which is east direction",
                //"How long is this path",
                //"Distance from here to there",
            };

            var searchClient = new SearchClient(
                               new Uri(settings.SearchEndpoint),
                               settings.searchIndexName,
                               new AzureKeyCredential(settings.SearchApiKey));

            var client = new AzureOpenAIClient(
                         new Uri(settings.EmbEndpoint),
                         new AzureKeyCredential(settings.EmbApiKey));

            var docs = new List<object>();

            foreach (var text in documents)
            {
                var emb = await GetEmbedding(client, text);

                docs.Add(new
                {
                    id = Guid.NewGuid().ToString(),
                    content = text,
                    vector = emb
                });
            }
            await searchClient.UploadDocumentsAsync(docs);
        }

        async Task<ReadOnlyMemory<float>> GetEmbedding(AzureOpenAIClient client, string text)
        {
            EmbeddingClient embeddingClient = client.GetEmbeddingClient(settings.EmbDeployment);

            var result = await embeddingClient.GenerateEmbeddingsAsync(new List<string> { text });

            OpenAIEmbedding embedding = result.Value[0];

            return embedding.ToFloats();
        }
    }
}
