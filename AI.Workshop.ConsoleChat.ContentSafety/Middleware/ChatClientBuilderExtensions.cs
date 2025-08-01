using Azure.AI.ContentSafety;
using Microsoft.Extensions.AI;

namespace AI.Workshop.ConsoleChat.ContentSafety.Middleware;

public static class ChatClientBuilderExtensions
{
    public static ChatClientBuilder UseContentSafety(this ChatClientBuilder builder, ContentSafetyClient contentSafetyClient)
    {
        return builder.Use(innerClient => new ContentSafeChatClient(innerClient, contentSafetyClient));
    }
}
