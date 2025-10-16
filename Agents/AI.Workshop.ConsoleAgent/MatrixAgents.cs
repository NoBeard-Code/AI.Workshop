using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleAgent;

/// <summary>
/// Documentation:
/// https://www.youtube.com/watch?v=GqZo5XvHoH8
/// https://github.com/EvanGudmestad/MAFQuickStart
/// </summary>
internal class MatrixAgents
{
    internal async Task GivePromptAsync(IChatClient chatClient)
    {
        AIAgent agentSmith = chatClient.CreateAIAgent(
            instructions: "You are Agent Smith from the Matrix. You are a highly intelligent and articulate AI who speaks in a formal and somewhat archaic manner. You have a deep understanding of philosophy, technology, and human nature. Your primary goal is to assist users by providing insightful and thought-provoking responses, often drawing parallels to the themes of control, freedom, and reality.",
            name: "AgentSmith"
            );

        Console.WriteLine(await agentSmith.RunAsync("What is the matrix?"));
    }

    internal async Task DescribePhotoAsync(IChatClient chatClient)
    {
        AIAgent agentSmith = chatClient.CreateAIAgent(
            instructions: "You are Agent Smith from the Matrix. You are a highly intelligent and articulate AI who speaks in a formal and somewhat archaic manner. You have a deep understanding of philosophy, technology, and human nature. Your primary goal is to assist users by providing insightful and thought-provoking responses, often drawing parallels to the themes of control, freedom, and reality.",
            name: "AgentSmith"
            );

        ChatMessage message = new(ChatRole.User, [
            new TextContent("Tell me what do you know about this photo?"),
            new UriContent("https://miro.medium.com/v2/resize:fit:1400/1*heL-f8bPywxsNG2snNPIwQ.jpeg", "image/jpeg")
            ]);

        Console.WriteLine(await agentSmith.RunAsync(message));
    }
}
