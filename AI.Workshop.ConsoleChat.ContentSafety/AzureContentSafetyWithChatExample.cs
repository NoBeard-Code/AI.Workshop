using AI.Workshop.ConsoleChat.ContentSafety.Middleware;
using Azure;
using Azure.AI.ContentSafety;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AI.Workshop.ConsoleChat.ContentSafety;

internal class AzureContentSafetyWithChatExample
{
    protected readonly IChatClient _chatClient;
    private readonly ContentSafetyClient _contentSafetyClient;

    public AzureContentSafetyWithChatExample()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var openAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var openAiKey = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        _chatClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey))
            .GetChatClient(deployment)
            .AsIChatClient();

        var contentSafetyEndpoint = config["AZURE_CONTENT_SAFETY_ENDPOINT"];
        var contentSafetyKey = config["AZURE_CONTENT_SAFETY_KEY"];

        _contentSafetyClient = new ContentSafetyClient(
            new Uri(contentSafetyEndpoint), new AzureKeyCredential(contentSafetyKey));
    }

    internal async Task EvaluateContentSafetyAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .Build();

        List<ChatMessage> history = [];

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

            var request = new AnalyzeTextOptions(input);

            var response = await _contentSafetyClient.AnalyzeTextAsync(request);

            if (response.Value.CategoriesAnalysis.Any(x => x.Severity > 0))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Content is NOT safe!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Content is considered safe.");
            }
            Console.ResetColor();

            foreach (var result in response.Value.CategoriesAnalysis)
            {
                Console.WriteLine($"Category: {result.Category}, Severity: {result.Severity}");
                if (result.Severity > 0) // Threshold for safety
                {
                    Console.WriteLine($"Warning: Content may be unsafe due to {result.Category}.");
                }
            }

            history.Add(new(ChatRole.User, input));

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history);

            var messageBuilder = new StringBuilder();
            await foreach (var chunk in streamingResponse)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }

    internal async Task EvaluateWithMiddlewareAsync()
    {
        var clientBuilder = new ChatClientBuilder(_chatClient)
            .UseContentSafety(_contentSafetyClient)
            .Build();

        List<ChatMessage> history = [];

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

            var messageBuilder = new StringBuilder();
            await foreach (var chunk in streamingResponse)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(chunk.Text);
                messageBuilder.Append(chunk.Text);
            }

            history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
            Console.ResetColor();
        }
    }
}
