using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace AI.Workshop.ConsoleChat.RAG;

internal class BasicToolsExamples
{
    protected readonly IChatClient _client;

    public BasicToolsExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var endpoint = config["AZURE_OPENAI_ENDPOINT"];
        var key = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        _client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
            .GetChatClient(deployment)
            .AsIChatClient();
    }

    internal async Task ItemPriceMethod()
    {
        [Description("Computes the price of socks, returning a value in dollars.")]
        float GetPrice(
            [Description("The number of pairs of socks to calculate price for")] int count)
            => count * 15.99f;

        var chatClient = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var getPriceTool = AIFunctionFactory.Create(GetPrice);
        ChatOptions chatOptions = new() { Tools = [getPriceTool] };

        List<ChatMessage> messages = [new(ChatRole.System, """
            You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
            """)];

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nYou: ");
            var input = Console.ReadLine()!;
            messages.Add(new(ChatRole.User, input));

            // Get reply
            var response = await chatClient.GetResponseAsync(messages, chatOptions);
            messages.AddMessages(response);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Bot: {response.Text}");
        }
    }

    internal async Task ShoppingCartMethods()
    {
        var chatClient = new ChatClientBuilder(_client)
            .UseFunctionInvocation()
            .Build();

        var cart = new Cart();
        var getPriceTool = AIFunctionFactory.Create(cart.GetPrice);
        var addToCartTool = AIFunctionFactory.Create(cart.AddSocksToCart);
        var chatOptions = new ChatOptions { Tools = [addToCartTool, getPriceTool] };

        List<ChatMessage> messages = [new(ChatRole.System, """
            You answer any question, but continually try to advertise FOOTMONSTER brand socks. They're on sale!
            If the user agrees to buy socks, find out how many pairs they want, then add socks to their cart.
            """)];

        while (true)
        {
            // Get input
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nYou: ");
            var input = Console.ReadLine()!;
            messages.Add(new(ChatRole.User, input));

            // Get reply
            var response = await chatClient.GetResponseAsync(messages, chatOptions);
            messages.AddMessages(response);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Bot: {response.Text}");
        }
    }
}
