using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var pat = config["GITHUB_TOKEN"];

IChatClient chatClient =
    new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(pat),
            new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
        .AsIChatClient();

AIAgent writer = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "Writer",
        Instructions = "Write stories that are engaging and creative.",
        ChatOptions = new ChatOptions
        {
            Tools = [
                AIFunctionFactory.Create(GetAuthor),
                AIFunctionFactory.Create(FormatStory)
            ],
        }
    });

//AgentRunResponse response = await writer.RunAsync("Write a short story about a haunted house.");
//Console.WriteLine(response.Text);

AIAgent editor = new ChatClientAgent(
    chatClient,
    new ChatClientAgentOptions
    {
        Name = "Editor",
        Instructions = "Make the story more engaging, fix grammar, and enhance the plot."
    });

// Create a workflow that connects writer to editor
Workflow workflow =
    AgentWorkflowBuilder
        .BuildSequential(writer, editor);

AIAgent workflowAgent = await workflow.AsAgentAsync();

AgentRunResponse workflowResponse =
    await workflowAgent.RunAsync("Write a short story about a haunted house.");

Console.WriteLine(workflowResponse.Text);

Console.ReadLine();

[Description("Gets the author of the story.")]
string GetAuthor() => "Jack Torrance";

[Description("Formats the story for display.")]
string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";