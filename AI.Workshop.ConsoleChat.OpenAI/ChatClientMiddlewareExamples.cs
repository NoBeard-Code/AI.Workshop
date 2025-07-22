using Microsoft.Extensions.AI;
using System.Threading.RateLimiting;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class ChatClientMiddlewareExamples : AzureOpenAIBase
{
    internal async Task BasicUsage()
    {
        var client = new RateLimitingChatClient(_client,
            new ConcurrencyLimiter(new()
            {
                PermitLimit = 1, // Limit to one concurrent request
                QueueLimit = int.MaxValue // No limit on the queue size
            }));

        Console.WriteLine(await client.GetResponseAsync("What color is the sky?"));
    }

    internal async Task UsageWithMiddleware()
    {
        var client = new ChatClientBuilder(_client)
            .UseRateLimiting(
                new ConcurrencyLimiter(new()
                {
                    PermitLimit = 1, // Limit to one concurrent request
                    QueueLimit = int.MaxValue // No limit on the queue size
                }))
            .Build();

        Console.WriteLine(await client.GetResponseAsync("What color is the sky?"));
    }

    internal async Task UsageWithoutCustomClass()
    {
        var rateLimiter = new ConcurrencyLimiter(new()
        {
            PermitLimit = 1, // Limit to one concurrent request
            QueueLimit = int.MaxValue // No limit on the queue size
        });

        var client = new ChatClientBuilder(_client)
            .Use(async (messages, options, nextAsync, cancellationToken) =>
            {
                // Acquire a lease from the rate limiter

                using var lease = await rateLimiter.AcquireAsync(permitCount: 1, cancellationToken)
                    .ConfigureAwait(false); // avoids deadlocks in some scenarios

                if (!lease.IsAcquired)
                    throw new InvalidOperationException("Unable to acquire lease.");

                await nextAsync(messages, options, cancellationToken);
            })
            .Build();

        Console.WriteLine(await client.GetResponseAsync("What color is the sky?"));
    }
}
