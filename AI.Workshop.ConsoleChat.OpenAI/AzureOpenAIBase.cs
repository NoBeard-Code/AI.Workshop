using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal abstract class AzureOpenAIBase
{
    protected readonly IChatClient _client;

    protected AzureOpenAIBase()
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
}
