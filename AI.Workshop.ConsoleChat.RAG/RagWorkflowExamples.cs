using AI.Workshop.ConsoleChat.RAG.Tools;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace AI.Workshop.ConsoleChat.RAG;

internal class RagWorkflowExamples
{
    protected readonly AzureOpenAIClient _innerClient;
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
        var searchEndpoint = config["AZURE_SEARCH_ENDPOINT"];
        var searchKey = config["AZURE_SEARCH_KEY"];

        var section = config.GetSection("Prompts:OpenAISystemPrompt");
        _systemPrompt = string.Join("", section.GetChildren().Select(x => x.Value));

        var client = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey));
        
        _innerClient = client;
        _configuration = config;

        _client = client
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

        AddToolDefinition("CurrentTimeToolPrompts", new CurrentTimeTool() as IChatTool);
        AddToolDefinition("AzureAISearchInhaltIndexToolPrompts", new AzureAISearchInhaltIndexToolMock()); 
        AddToolDefinition("AzureAISearchKnowledgeBaseToolPrompts", new AzureAISearchKnowledgeBaseToolMock());

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

    internal async Task RagWithSearchToolsByDefaultAsync(string userPrompt)
    {
        var clientBuilder = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        List<ChatMessage> history = [new(ChatRole.System, _systemPrompt)];

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(_systemPrompt);
        Console.ResetColor();

        AddToolDefinition("CurrentTimeToolPrompts", new CurrentTimeTool() as ISearchChatTool);
        AddToolDefinition("AzureAISearchInhaltIndexToolPrompts", new AzureAISearchInhaltIndexTool(_innerClient, _configuration));
        AddToolDefinition("AzureAISearchKnowledgeBaseToolPrompts", new AzureAISearchKnowledgeBaseTool(_innerClient, _configuration));

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\nQ: {userPrompt}");
        history.Add(new(ChatRole.User, userPrompt));
        
        var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

        var messageBuilder = new StringBuilder();
        await foreach (var update in streamingResponse)
        {
            if (update.FinishReason == ChatFinishReason.ToolCalls)
            {
                foreach (var functionCall in update.Contents.OfType<FunctionCallContent>())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nTool Call: {functionCall.Name}");

                    var parameters = functionCall.Arguments;
                    var json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(json);
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(update.Text);
            messageBuilder.Append(update.Text);
        }

        history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
        Console.ResetColor();
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

        //history.Add(new(ChatRole.Assistant, "“Tools must be invoked using all relevant user intent. Parameters for tool methods are inferred from semantic labels and descriptions.”"));

        var summary = AggregateHistoryToString(history);

        AddToolDefinitionFixed("CurrentTimeToolPrompts", new CurrentTimeTool());
        AddToolDefinitionFixed("AzureAISearchInhaltIndexToolPrompts", new AzureAISearchInhaltIndexTool(_innerClient, _configuration));
        AddToolDefinitionFixed("AzureAISearchKnowledgeBaseToolPrompts", new AzureAISearchKnowledgeBaseTool(_innerClient, _configuration));

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\nQ: {userPrompt}");
        history.Add(new(ChatRole.User, userPrompt));

        var streamingResponse = clientBuilder.GetStreamingResponseAsync(history, _chatOptions);

        var messageBuilder = new StringBuilder();
        await foreach (var update in streamingResponse)
        {
            if (update.FinishReason == ChatFinishReason.ToolCalls)
            {
                foreach (var functionCall in update.Contents.OfType<FunctionCallContent>())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nTool Call: {functionCall.Name}");

                    //functionCall.Arguments.TryGetValue("query", out var query);
                    //query = summary ?? query?.ToString();
                    //functionCall.Arguments.TryGetValue("top", out var top);
                    //top = 5;

                    //functionCall.Arguments!["query"] = summary;
                    //functionCall.Arguments["top"] = 5;

                    var parameters = functionCall.Arguments;
                    var json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(json);
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(update.Text);
            messageBuilder.Append(update.Text);
        }

        history.Add(new(ChatRole.Assistant, messageBuilder.ToString()));
        Console.ResetColor();
    }

    private static string AggregateHistoryToString(List<ChatMessage> history)
    {
        var sb = new StringBuilder();

        foreach (var message in history)
        {
            sb.AppendLine($"[{message.Role}] {message.Text}");
        }

        return sb.ToString();
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

        //Dictionary<string, object> parameters = [];

        //var queryParameter = section.GetSection("queryParameterDescription").GetChildren();

        //if (queryParameter.Any())
        //{
        //    parameters.Add("query", string.Join("", queryParameter.Select(x => x.Value)));
        //}
        
        //var topParameter = section.GetSection("topParameterDescription").GetChildren();

        //if (topParameter.Any()) {
        //    parameters.Add("top", string.Join("", topParameter.Select(x => x.Value)));
        //}

        //factoryOptions.AdditionalProperties = parameters;

        var aiFunction = AIFunctionFactory.Create(
            method: tool.InvokeAsync,
            factoryOptions);

        _chatOptions.Tools!.Add(aiFunction);
    }

    private void AddToolDefinition(string sectionName, ISearchChatTool tool)
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

        var aiFunction = AIFunctionFactory.Create(
            method: tool.SearchDocumentsWithQueryAndTop,
            factoryOptions);

        _chatOptions.Tools!.Add(aiFunction);
    }

    private void AddToolDefinitionFixed(string sectionName, ISearchChatTool tool)
    {
        var section = _configuration.GetSection(sectionName);

        if (section == null || !section.Exists())
        {
            throw new ArgumentException($"Tool definition section '{sectionName}' not found in configuration.");
        }

        //var schema = BinaryData.FromString("""
        //{
        //    "type": "object",
        //    "properties": {
        //        "Movies": {
        //            "type": "array",
        //            "items": {
        //                "type": "object",
        //                "properties": {
        //                    "Title": { "type": "string" },
        //                    "Director": { "type": "string" },
        //                    "ReleaseYear": { "type": "integer" },
        //                    "Rating": { "type": "number" },
        //                    "IsAvailableOnStreaming": { "type": "boolean" },
        //                    "Tags": { "type": "array", "items": { "type": "string" } }
        //                },
        //                "required": ["Title", "Director", "ReleaseYear", "Rating", "IsAvailableOnStreaming", "Tags"],
        //                "additionalProperties": false
        //            }
        //        }
        //    },
        //    "required": ["Movies"],
        //    "additionalProperties": false
        //}
        //""");

        var factoryOptions = new AIFunctionFactoryOptions
        {
            Name = section["Name"],
            Description = string.Join("", section.GetSection("Description").GetChildren().Select(x => x.Value)),
            //ConfigureParameterBinding = (parameter) =>
            //{
            //    var options = new ParameterBindingOptions
            //    {
            //        BindParameter = (param, context) =>
            //        {
            //            if (param.Name == "query")
            //            {
            //                return context.GetValueOrDefault("query") ?? "default query";
            //            }
            //            else if (param.Name == "top")
            //            {
            //                return context.GetValueOrDefault("top") ?? 5;
            //            }
            //            return null;
            //        },
            //    };

            //    return options;
            //},
        };

        var aiFunction = AIFunctionFactory.Create(
            method: tool.SearchDocumentsWithQueryAndTop,
            factoryOptions);

        //tool.InvokeAsync(new AIFunctionArguments {
        //    ["parameters"] = new Dictionary<string, object>
        //    {
        //        ["query"] = "I'm testing. Fetch me one article from your knowledge base, a seminar from inhalt index and tell me the current time.",
        //        ["top"] = 5
        //    }
        //});

        _chatOptions.Tools!.Add(aiFunction);
    }
}
