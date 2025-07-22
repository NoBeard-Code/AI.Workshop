using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleChat.OpenAI;

/// <summary>
/// Azure OpenAI is stateless by default, 
/// but offers stateful capabilities when you opt into features like Assistants API or stored completions.
/// </summary>
internal class StatefulServiceExamples : AzureOpenAIBase
{
    internal async Task SendStatelessPrompts()
    {
        List<ChatMessage> history = [];
        while (history.Count < 3)
        {
            Console.Write("Q: ");
            history.Add(new(ChatRole.User, Console.ReadLine()));

            var response = await _client.GetResponseAsync(history);
            Console.WriteLine(response);

            history.AddMessages(response);
        }
    }

    internal async Task SendStatefulPromptsExplicitly()
    {
        List<ChatMessage> history = [];
        ChatOptions chatOptions = new();

        while (history.Count <= 3)
        {
            Console.Write("Q: ");
            history.Add(new(ChatRole.User, Console.ReadLine()));

            var response = await _client.GetResponseAsync(history);           
            Console.WriteLine(response);

            chatOptions.ConversationId = response.ConversationId;
            if (response.ConversationId is not null)
            {
                history.Clear();
            }
            else
            {
                history.AddMessages(response);
            }
        }
    }

    internal async Task SendStatefulPromptsImplicitly()
    {
        string GetCurrentWeather() => Random.Shared.NextDouble() > 0.5 ? "It's sunny" : "It's raining";

        var client = new ChatClientBuilder(_client)
            .UseFunctionInvocation() // Enables stateful conversation by default
            .Build();

        ChatOptions options = new() { Tools = [AIFunctionFactory.Create(GetCurrentWeather)] };

        var response1 = await client.GetResponseAsync("What is the weather in Varaždin?", options);
        Console.WriteLine(response1.Text);

        var response2 = await client.GetResponseAsync("What's the town I just mentioned for a weather?", options);
        Console.WriteLine(response2.Text);
    }
}
