using OpenAI.Chat;

namespace OpenAiRagApp.Services
{
    public interface IChatBotService
    {
        Task<string> GetRagChatResponseAsync(string context, string question);
       Task FreeChatAsync();
    }
}
