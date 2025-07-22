using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleChat.OpenAI;

// https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/prompt-model?pivots=azure-openai

internal class BasicChatExamples : AzureOpenAIBase
{
    internal async Task HelloPrompt()
    {
        var text = File.ReadAllText("benefits.md");

        var prompt = $"""
            Summarize the the following text in 20 words or less:
            {text}
            """;

        var response = await _client.GetResponseAsync(prompt, new ChatOptions { MaxOutputTokens = 400 });

        Console.WriteLine(response);
    }

    internal async Task RequestChatResponse()
    {
        var response = await _client.GetResponseAsync(
        [
            new(ChatRole.System, "You are a very skillful chef."),
            new(ChatRole.User, "What should I cook today?"),
        ]);

        Console.WriteLine(response);
    }

    internal async Task RequestChatResponseWithHistory()
    {
        List<ChatMessage> history = [];
        while (history.Count < 3)
        {
            Console.Write("Q: ");
            history.Add(new(ChatRole.User, Console.ReadLine()));

            var response = await _client.GetResponseAsync(history);
            Console.WriteLine(response);

            history.AddMessages(response); // extension method to add messages to history!
        }
    }

    internal async Task RequestStreamingChatResponse()
    {
        await foreach (ChatResponseUpdate update in _client.GetStreamingResponseAsync("What is AI?"))
        {
            Console.Write(update);
        }

        Console.WriteLine();    
    }

    internal async Task RequestStreamingChatResponseWithHistory()
    {
        List<ChatMessage> chatHistory = [];
        while (chatHistory.Count < 3)
        {
            Console.Write("Q: ");
            chatHistory.Add(new(ChatRole.User, Console.ReadLine()));

            List<ChatResponseUpdate> updates = [];
            await foreach (ChatResponseUpdate update in _client.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(update);
                updates.Add(update);
            }
            Console.WriteLine();

            // Helpers like AddMessages compose a ChatResponse and then extract the composed messages from the response and add them to a list.

            chatHistory.AddMessages(updates); // extension method to add messages to history!
        }
    }
}
