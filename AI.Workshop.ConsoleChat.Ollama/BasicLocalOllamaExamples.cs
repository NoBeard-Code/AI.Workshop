using Microsoft.Extensions.AI;
using OllamaSharp;
using System.Text;

namespace AI.Workshop.ConsoleChat.Ollama;

internal class BasicLocalOllamaExamples
{
    private readonly IChatClient _chatClient;

    public BasicLocalOllamaExamples()
    {
        var ollamaUri = new Uri("http://localhost:11434/");
        var ollamaModel = "llama3.2";

        _chatClient = new OllamaApiClient(ollamaUri, ollamaModel);
    }

    internal async Task BasicPromptAsync()
    {
        List<ChatMessage> chatHistory = new();

        while (true)
        {
            // Get user prompt and add to chat history
            Console.Write("Q: ");
            var userPrompt = Console.ReadLine();
            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

            // Stream the AI response and add to chat history
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("A: ");
            var response = "";
            await foreach (ChatResponseUpdate item in _chatClient.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                response += item.Text;
            }
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
            Console.WriteLine();
            Console.ResetColor();
        }
    }

    internal async Task BasicPromptWithSystemMessageAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .Build();

        var systemPrompt = "You are a helpful assistant who suggests whick books to read.";
        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(systemPrompt);
        Console.ResetColor();

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nQ: ");
            var input = Console.ReadLine()!;

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Exiting chat.");
                Console.ResetColor();
                break;
            }

            history.Add(new(ChatRole.User, input));

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("A: ");
            var messageBuilder = new StringBuilder();

            await foreach (var chunk in streamingResponse)
            {
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

}
