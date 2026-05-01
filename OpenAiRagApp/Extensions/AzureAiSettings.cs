using System.ComponentModel.DataAnnotations;

namespace OpenAiRagApp.Extensions
{
    internal class AzureAiSettings
    {
        public const string SectionName = "AzureOpenAiSettings";

        [Required, Url]
        public string AzureOpenAiEndpoint { get; set; } = string.Empty;

        [Required]
        public string AzureOpenAiApiKey { get; set; } = string.Empty;

        [Required]
        public string ChatModelDeployment { get; set; } = string.Empty;

        [Required]
        public string EmbeddingModelDeployment { get; set; } = string.Empty;

        [Required]
        public string SearchApiKey { get; set; } = string.Empty;

        [Required, Url]
        public string SearchEndpoint { get; set; } = string.Empty;

        [Required]
        public string SearchIndexName { get; set; } = string.Empty;
    }
}
