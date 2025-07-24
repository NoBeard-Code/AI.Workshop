using AI.Workshop.ConsoleChat.RAG.Tools;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AI.Workshop.ConsoleChat.RAG;

internal class RagWorkflowExamples
{
    protected readonly IChatClient _client;
    private readonly string _systemPrompt;

    public RagWorkflowExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("Prompts.json", false, false)
            .Build();

        var openAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var openAiKey = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        var section = config.GetSection("Prompts:OpenAISystemPrompt");
        _systemPrompt = string.Join("", section.GetChildren().Select(x => x.Value));
        
        _client = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey))
            .GetChatClient(deployment)
            .AsIChatClient();
    }

    internal async Task InitialMessageLoopAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
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

            ChatOptions options = new() {
                Temperature = 0.2f,
                MaxOutputTokens = 1000,
                FrequencyPenalty = 0.1f,
                PresencePenalty = 0.0f,
                TopP = 0.3f,
                ToolMode = ChatToolMode.Auto
            };
            
            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, options);

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

    internal async Task RagWithToolsAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        ChatOptions options = new()
        {
            Temperature = 0.2f,
            MaxOutputTokens = 1000,
            FrequencyPenalty = 0.1f,
            PresencePenalty = 0.0f,
            TopP = 0.3f,
            ToolMode = ChatToolMode.Auto,
            Tools = []
        };

        var currentTimeTool = AIFunctionFactory.Create(CurrentTimeTool.GetTime);

        options.Tools.Add(currentTimeTool);

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

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, options);

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
