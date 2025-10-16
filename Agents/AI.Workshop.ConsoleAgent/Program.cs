using AI.Workshop.ConsoleAgent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var token = config["GITHUB_TOKEN"];

IChatClient chatClient =
    new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(token),
            new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
        .AsIChatClient();

//var writers = new GhostWriterAgents();
//await writers.RunAsync(chatClient);

var matrix = new MatrixAgents();
//await matrix.GivePromptAsync(chatClient);
await matrix.DescribePhotoAsync(chatClient);

