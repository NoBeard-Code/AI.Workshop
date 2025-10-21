using AI.Workshop.ConsoleAgent;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

//var token = config["GITHUB_TOKEN"];

//IChatClient chatClient =
//    new ChatClient(
//            "gpt-41-mini",
//            new ApiKeyCredential(token),
//            new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
//        .AsIChatClient();

var token = config["AZURE_OPENAI_KEY"]!;
var endpoint = config["AZURE_OPENAI_ENDPOINT"]!;
var model = config["AZURE_OPENAI_DEPLOYMENT"]!;

var openAIClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(token));

var chatClient = openAIClient
    .GetChatClient(model)
    .AsIChatClient();

var writers = new GhostWriterAgents();
await writers.RunAsync(chatClient);

//var matrix = new MatrixAgents();
//await matrix.GivePromptAsync(chatClient);
//await matrix.DescribePhotoAsync(chatClient);
//await matrix.MultiTurnConversationAsync(chatClient);
//await matrix.FunctionCallingAsync(chatClient);
//await matrix.StructuredOutputAsync(chatClient);
//await matrix.UseAgentAsToolAsync(chatClient);

