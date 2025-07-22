using Microsoft.Extensions.AI;
using System.Threading.RateLimiting;

namespace AI.Workshop.ConsoleChat.OpenAI;

public static class ChatClientBuilderExtensions
{
    public static ChatClientBuilder UseRateLimiting(this ChatClientBuilder builder, RateLimiter rateLimiter)
    {
        return builder.Use(innerClient => new RateLimitingChatClient(innerClient, rateLimiter));
    }
}
