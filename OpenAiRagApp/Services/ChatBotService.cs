using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using OpenAiRagApp.Extensions;

namespace OpenAiRagApp.Services
{
    internal class ChatBotService(IOptions<AzureAiSettings> settings,
                                  AzureOpenAIClient client) : IChatBotService
    {
        public async Task<string> GetRagChatResponseAsync(string context, string question)
        {
            var chatClient = client.GetChatClient(settings.Value.ChatModelDeployment);

            var messages = new List<ChatMessage>
                           {
                                ChatMessage.CreateSystemMessage(
                                    "You are a helpful assistant. Answer ONLY from the given context."),

                                ChatMessage.CreateUserMessage($@"
                                        Context:
                                        {context}

                                        Question:
                                        {question}
                                        ")
                            };
            var options = new ChatCompletionOptions()
            {
                Temperature = 0.5f,
                FrequencyPenalty = 0f,
                PresencePenalty = 0f
            };
            var response = await chatClient.CompleteChatAsync(messages, options);

            return response.Value.Content[0].Text;
        }

        public async Task FreeChatAsync()
        {
            Console.WriteLine("Chat bot Ready (type '0' to quit)");
            string chatMode = Environment.GetEnvironmentVariable("CHAT_MODE") ?? "";

            while (true)
            {
                Console.Write("\nUser: ");
                var query = Console.ReadLine();
                if (!string.IsNullOrEmpty(query))
                {
                    if (query == "0") break;

                    var chatClient = client.GetChatClient(settings.Value.ChatModelDeployment);

                    var options = new ChatCompletionOptions()
                                  {
                                      Temperature = 0.5f,
                                      FrequencyPenalty = 0f,
                                      PresencePenalty = 0f
                                  };
                    var messages = new List<ChatMessage>
                                   {
                                        ChatMessage.CreateSystemMessage(
                                            "You are an AI assistant that helps users get accurate information. All responses in 50 words only."),

                                        ChatMessage.CreateUserMessage(query)
                                   };

                    if (chatMode == "Non-Streaming")
                    {
                        //For non-streaming response
                        await FreeNonStreamChatAsync(chatClient, options, messages);
                    }
                    else  
                    {
                        //For streaming response
                        await FreeStreamChatAsync(chatClient, options, messages);
                    }
                }
            }
        }

        private static async Task FreeStreamChatAsync(ChatClient chatClient, ChatCompletionOptions options, List<ChatMessage> messages)
        {           
            var updates = chatClient.CompleteChatStreamingAsync(messages, options);

            Console.WriteLine("\nAI: ");

            await foreach (StreamingChatCompletionUpdate update in updates)
            {
                foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
                {
                    Console.Write(updatePart.Text);
                }
            }
            Console.WriteLine();
        }

        private static async Task FreeNonStreamChatAsync(ChatClient chatClient, ChatCompletionOptions options, List<ChatMessage> messages)
        {
            var response = await chatClient.CompleteChatAsync(messages, options);

            Console.WriteLine($"\nAI: {response.Value.Content[0].Text}");
        }

    }
}
