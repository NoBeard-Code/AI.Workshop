using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class BasicChatExamples
{
    private readonly IChatClient _client;

    internal BasicChatExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var endpoint = config["AZURE_OPENAI_ENDPOINT"];
        var key = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
            .GetChatClient(deployment)
            .AsIChatClient();
    }

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
