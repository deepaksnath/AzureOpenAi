namespace OpenAiRagApp
{
    internal class AzureAiSettings
    {
        public string ChatEndpoint { get; set; } = string.Empty;
        public string ChatDeployment { get; set; } = string.Empty;
        public string ChatApiKey { get; set; } = string.Empty;
        public string EmbEndpoint { get; set; } = string.Empty;
        public string EmbDeployment { get; set; } = string.Empty;
        public string EmbApiKey { get; set; } = string.Empty;
        public string SearchApiKey { get; set; } = string.Empty;
        public string SearchEndpoint { get; set; } = string.Empty;
        public string searchIndexName { get; set; } = string.Empty;
    }
}
