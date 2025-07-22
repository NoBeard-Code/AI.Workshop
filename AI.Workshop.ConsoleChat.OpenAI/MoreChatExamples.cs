using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class MoreChatExamples : AzureOpenAIBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-chat-app?pivots=azure-openai"/>
    /// <returns></returns>
    internal async Task HikingChat()
    {
        // Create a system prompt to provide the AI model with initial role context and instructions about hiking recommendations:

        List<ChatMessage> chatHistory = [ new ChatMessage(ChatRole.System, """
            You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
            You introduce yourself when first saying hello.
            When helping people out, you always ask them for this information
            to inform the hiking recommendation you provide:

            1. The location where they would like to hike
            2. What hiking intensity they are looking for

            You will then provide three suggestions for nearby hikes that vary in length
            after you get that information. You will also share an interesting fact about
            the local nature on the hikes when making a recommendation. At the end of your
            response, ask if there is anything else you can help with.
        """)];

        Console.WriteLine(await _client.GetResponseAsync(chatHistory));

        // Loop to get user input and stream AI response
        while (chatHistory.Count <= 3)
        {
            // Get user prompt and add to chat history
            Console.Write("Your prompt: ");
            string? userPrompt = Console.ReadLine();
            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response:");
            string response = "";
            await foreach (ChatResponseUpdate item in _client.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                response += item.Text;
            }
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
            Console.WriteLine();
        }
    }

    private enum Sentiment
    {
        Positive,
        Negative,
        Neutral
    };

    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/structured-output"/>"/>
    /// <returns></returns>
    internal async Task RequestStructuredOutput()
    {
        string review = "I'm happy with the product!";

        var response = await _client.GetResponseAsync<Sentiment>($"What's the sentiment of this review? {review}");

        Console.WriteLine($"Sentiment: {response.Result}");
    }

    internal async Task SentimentAnalysisForMoreInputs()
    {
        string[] inputs = [
            "Best purchase ever!",
            "Returned it immediately.",
            "Hello",
            "It works as advertised.",
            "The packaging was damaged but otherwise okay."
        ];

        foreach (var i in inputs)
        {
            var response2 = await _client.GetResponseAsync<Sentiment>($"What's the sentiment of this review? {i}");
            Console.WriteLine($"Review: {i} | Sentiment: {response2.Result}");
        }
    }
}
