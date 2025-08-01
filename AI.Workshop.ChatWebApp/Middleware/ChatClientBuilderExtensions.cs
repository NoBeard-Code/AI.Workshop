using Azure.AI.ContentSafety;
using Microsoft.Extensions.AI;
using System.Threading.RateLimiting;

namespace AI.Workshop.ChatWebApp.Middleware;

public static class ChatClientBuilderExtensions
{
    public static ChatClientBuilder UseRateLimiting(this ChatClientBuilder builder, RateLimiter rateLimiter)
    {
        return builder.Use(innerClient => new RateLimitingChatClient(innerClient, rateLimiter));
    }

    public static ChatClientBuilder UseContentSafety(this ChatClientBuilder builder, ContentSafetyClient contentSafetyClient)
    {
        return builder.Use(innerClient => new ContentSafeChatClient(innerClient, contentSafetyClient));
    }
}
