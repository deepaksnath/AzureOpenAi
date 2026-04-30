using Azure.AI.OpenAI;
using Azure.Search.Documents;

namespace OpenAiRagApp.Services
{
    public interface ISemanticSearchService
    {
        Task InitializeUploadAsync();
        Task FreeSemanticSerachAsync();
        Task UploadDocumentsAsync();
        Task<ReadOnlyMemory<float>> GetEmbeddingAsync(AzureOpenAIClient client, string text);
        Task<List<string>> SearchDocumentsAsync(SearchClient searchClient, ReadOnlyMemory<float> queryEmbedding);
    }
}
