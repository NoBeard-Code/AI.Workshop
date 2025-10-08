using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var token = builder.Configuration["GITHUB_TOKEN"];

IChatClient chatClient =
    new ChatClient(
            "gpt-4o-mini",
            new ApiKeyCredential(token!),
            new OpenAIClientOptions { Endpoint = new Uri("https://models.github.ai/inference") })
        .AsIChatClient();

builder.Services.AddChatClient(chatClient);

builder.AddAIAgent("Writer", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return new ChatClientAgent(
        chatClient,
        name: key,
        instructions:
            """
            You are a creative writing assistant who crafts vivid, 
            well-structured stories with compelling characters based on user prompts, 
            and formats them after writing.
            """,
        tools: [
            AIFunctionFactory.Create(GetAuthor),
            AIFunctionFactory.Create(FormatStory)
        ]
    );
});

builder.AddAIAgent(
    name: "Editor",
    instructions:
        """
        You are an editor who improves a writer’s draft by providing 4–8 concise recommendations and 
        a fully revised Markdown document, focusing on clarity, coherence, accuracy, and alignment.
        """);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    app.UseDefaultFiles(); // Looks for index.html by default
    app.UseStaticFiles(); // Enable static file serving from wwwroot
}

app.UseHttpsRedirection();

app.MapGet("/agent/chat", async (
    [FromKeyedServices("Writer")] AIAgent writer,
    [FromKeyedServices("Editor")] AIAgent editor,
    HttpContext context,
    string prompt) =>
{
    Workflow workflow =
        AgentWorkflowBuilder
            .CreateGroupChatBuilderWith(agents =>
                new AgentWorkflowBuilder.RoundRobinGroupChatManager(agents)
                {
                    MaximumIterationCount = 2
                })
            .AddParticipants(writer, editor)
            .Build();

    AIAgent workflowAgent = await workflow.AsAgentAsync();

    AgentRunResponse response = await workflowAgent.RunAsync(prompt);
    return Results.Ok(response);
});

app.Run();

[Description("Gets the author of the story.")]
string GetAuthor() => "Jack Torrance";

[Description("Formats the story for display.")]
string FormatStory(string title, string author, string story) =>
    $"Title: {title}\nAuthor: {author}\n\n{story}";