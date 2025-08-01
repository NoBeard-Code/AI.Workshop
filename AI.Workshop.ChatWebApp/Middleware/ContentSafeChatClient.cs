using Azure.AI.ContentSafety;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace AI.Workshop.ChatWebApp.Middleware;

internal sealed class ContentSafeChatClient(IChatClient innerClient, ContentSafetyClient contentSafetyClient) 
    : DelegatingChatClient(innerClient)
{
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (!await IsContentSafeAsync(messages)) return new ChatResponse();

        // Here you would implement your content safety checks
        // For demonstration, we will just call the inner client directly
        return await base.GetResponseAsync(messages, options, cancellationToken)
            .ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!await IsContentSafeAsync(messages)) yield return new ChatResponseUpdate();

        // Here you would implement your content safety checks
        // For demonstration, we will just call the inner client directly
        await foreach (var update in base.GetStreamingResponseAsync(messages, options, cancellationToken)
            .ConfigureAwait(false))
        {
            yield return update;
        }
    }

    private async Task<bool> IsContentSafeAsync(IEnumerable<ChatMessage> messages)
    {
        var lastMessage = messages.LastOrDefault();

        if (lastMessage == null || string.IsNullOrWhiteSpace(lastMessage.Text))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("No content to analyze for safety.");
            return true;
        }

        var request = new AnalyzeTextOptions(lastMessage.Text);

        var response = await contentSafetyClient.AnalyzeTextAsync(request);

        var isSafe = false;
        if (response.Value.CategoriesAnalysis.Any(x => x.Severity > 0))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Content is NOT safe!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Content is considered safe.");
            isSafe = true;
        }
        Console.ResetColor();

        foreach (var result in response.Value.CategoriesAnalysis)
        {
            Console.WriteLine($"Category: {result.Category}, Severity: {result.Severity}");
            if (result.Severity > 0) // Threshold for safety
            {
                Console.WriteLine($"Warning: Content may be unsafe due to {result.Category}.");
            }
        }

        return isSafe;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose any resources if necessary
        }
        base.Dispose(disposing);
    }
}
