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
    private readonly IConfigurationRoot _configuration;

    private readonly string _systemPrompt;

    private readonly ChatOptions _chatOptions = new()
    {
        Temperature = 0.2f,
        MaxOutputTokens = 1000,
        FrequencyPenalty = 0.1f,
        PresencePenalty = 0.0f,
        TopP = 0.3f,
        ToolMode = ChatToolMode.Auto,
        Tools = []
    };

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

        _configuration = config;
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
           
            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

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

    internal async Task RagWithBasicToolAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        var currentTime = new CurrentTimeTool();
        var currentTimeTool = AIFunctionFactory.Create(
            method: currentTime.InvokeAsync,
            name: "CurrentTime",
            description: "Returns the current date and time for Central European Time Zone. This tool needs no parameters.");

        //var currentTimeTool = AIFunctionFactory.Create(CurrentTimeTool.InvokesAsync);

        _chatOptions.Tools!.Add(currentTimeTool);

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

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

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

    internal async Task RagWithToolDefinitionsAsync()
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        AddToolDefinition("CurrentTimeToolPrompts", new CurrentTimeTool());
        AddToolDefinition("AzureAISearchInhaltIndexToolPrompts", new AzureAISearchInhaltIndexTool());
        AddToolDefinition("AzureAISearchKnowledgeBaseToolPrompts", new AzureAISearchKnowledgeBaseTool());

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

            var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

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

    internal async Task RagWithSearchToolsAsync(string userPrompt)
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        AddToolDefinition("CurrentTimeToolPrompts", new CurrentTimeTool());
        AddToolDefinition("AzureAISearchInhaltIndexToolPrompts", new AzureAISearchInhaltIndexTool());
        AddToolDefinition("AzureAISearchKnowledgeBaseToolPrompts", new AzureAISearchKnowledgeBaseTool());

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\nQ: {userPrompt}");
        history.Add(new(ChatRole.User, userPrompt));

        var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

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

    private void AddToolDefinition(string sectionName, IChatTool tool)
    {
        var section = _configuration.GetSection(sectionName);

        if (section == null || !section.Exists())
        {
            throw new ArgumentException($"Tool definition section '{sectionName}' not found in configuration.");
        }

        var factoryOptions = new AIFunctionFactoryOptions
        {
            Name = section["Name"],
            Description = string.Join("", section.GetSection("Description").GetChildren().Select(x => x.Value))
        };

        Dictionary<string, object> parameters = [];

        var queryParameter = section.GetSection("queryParameterDescription").GetChildren();

        if (queryParameter.Any())
        {
            parameters.Add("query", string.Join("", queryParameter.Select(x => x.Value)));
        }
        
        var topParameter = section.GetSection("topParameterDescription").GetChildren();

        if (topParameter.Any()) {
            parameters.Add("top", string.Join("", topParameter.Select(x => x.Value)));
        }
        
        factoryOptions.AdditionalProperties = parameters;

        var aiFunction = AIFunctionFactory.Create(
            method: tool.InvokeAsync,
            factoryOptions);

        _chatOptions.Tools!.Add(aiFunction);
    }
}
