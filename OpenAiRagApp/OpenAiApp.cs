using OpenAiRagApp.Services;
using static OpenAiRagApp.Extensions.Constants;

namespace OpenAiRagApp
{
    internal class OpenAiApp(IRagHandlerService _ragHandlerService,
                             IChatBotService _chatBotService,
                             ISemanticSearchService _semanticSearchService)
    {
        public async Task Run(string action, bool isSeedingNeeded)
        {

            if (isSeedingNeeded)
            {
                //Only for seeding data to search index for demo purpose.
                await _semanticSearchService.InitializeUploadAsync();
            }

            Task task = action switch
            {
                //For free chat without RAG, which means the question will be directly sent to
                //Azure OpenAI without using the retrieved information from semantic search.
                nameof(Mode.CHAT) => _chatBotService.FreeChatAsync(),

                //For free semantic search, which means you can test and see the retrieved information
                //from semantic search based on your query without sending it to Azure OpenAI.
                nameof(Mode.SEARCH) => _semanticSearchService.FreeSemanticSearchAsync(),

                //For complete RAG demo, which includes semantic search
                //and then using the retrieved information to answer the question.
                nameof(Mode.RAG) => _ragHandlerService.RunRagHandlerAsync(),

                _ => Task.Run(() => Console.WriteLine("Invalid mode. Please choose Chat, Search, or RAG."))
            };

            await task;
        }
    }
}
