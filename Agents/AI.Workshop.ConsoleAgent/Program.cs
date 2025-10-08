using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var token = config["GITHUB_TOKEN"];

//var writers = new GhostWriterAgents();
//await writers.RunAsync(token);

// https://www.youtube.com/watch?v=GqZo5XvHoH8

IChatClient chatClient =
    new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(token),
            new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
        .AsIChatClient();

AIAgent agent = chatClient.CreateAIAgent();

Microsoft.Extensions.AI.ChatMessage message = new(ChatRole.User, [
    new TextContent("Tell me what do you know about this photo?"),
    new UriContent("https://en.wikipedia.org/wiki/G._K._Chesterton#/media/File:Gilbert_Chesterton.jpg", "image/jpg")
]);

Console.WriteLine(await agent.RunAsync(message));

