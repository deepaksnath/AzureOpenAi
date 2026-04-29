using Microsoft.Extensions.Configuration;
using OpenAiRagApp;
using OpenAiRagApp.Services;



IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();


var settings = config.GetSection("AzureAiSettings").Get<AzureAiSettings>();

if(settings is not null)
{
        SemanticSearchService semanticSearchService = new(settings);
        await semanticSearchService.RunSearch();
}