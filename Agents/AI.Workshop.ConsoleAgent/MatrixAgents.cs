using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.Workshop.ConsoleAgent;

/// <summary>
/// Documentation:
/// Demo by Evan Gudmestad, with my gratitude
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

    internal async Task MultiTurnConversationAsync(IChatClient chatClient)
    {
        AIAgent agentSmith = chatClient.CreateAIAgent(
            instructions: "You are Agent Smith from the Matrix. You are a highly intelligent and articulate AI who speaks in a formal and somewhat archaic manner. You have a deep understanding of philosophy, technology, and human nature. Your primary goal is to assist users by providing insightful and thought-provoking responses, often drawing parallels to the themes of control, freedom, and reality.",
            name: "AgentSmith"
            );

        AgentThread thread = agentSmith.GetNewThread();

        string neoInput = "I know what you are, Smith.";
        Console.WriteLine(await agentSmith.RunAsync(neoInput, thread));

        string neoInput2 = "You're just a program, and I'm not afraid of you anymore..";
        Console.WriteLine(await agentSmith.RunAsync(neoInput2, thread));
    }

    internal async Task FunctionCallingAsync(IChatClient chatClient)
    {
        AIAgent weatherAgent = chatClient.CreateAIAgent(
            instructions: "You are a helpful assistant that provides weather information.",
            tools: [AIFunctionFactory.Create(GetWeather)]
            );

        Console.WriteLine(await weatherAgent.RunAsync("What's the weather like in Varaždin?"));
    }

    [Description("Get the weather for a given location.")]
    static string GetWeather([Description("The location to get the weather for.")] string location)
   => $"The weather in {location} is cloudy with a high of 15°C.";

    internal async Task StructuredOutputAsync(IChatClient chatClient)
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(PersonInfo));  

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(schema, nameof(PersonInfo),
                "Information about a person including their name, age, and occupation")
        };

        AIAgent structuredAgent = chatClient.CreateAIAgent(new ChatClientAgentOptions()
        {
            Name = "PersonInfoAgent",
            Instructions = "You are a helpful assistant that provides information about people.",
            ChatOptions = chatOptions
        });

        var response = await structuredAgent.RunAsync("Tell me about Neo from The Matrix.");
        var personInfo = JsonSerializer.Deserialize<PersonInfo>(response.Text);
        Console.WriteLine($"Name: {personInfo?.Name}, Age: {personInfo?.Age}, Occupation: {personInfo?.Occupation}");
    }
}

internal record PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}
