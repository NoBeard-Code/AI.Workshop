using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var key = config["AZURE_OPENAI_KEY"];
var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
    .GetChatClient(deployment)
    .AsIChatClient();

var text = File.ReadAllText("benefits.md");

var prompt = $"""
    Summarize the the following text in 20 words or less:
    {text}
    """;

var response = await client.GetResponseAsync(prompt, new ChatOptions { MaxOutputTokens = 400 });

Console.WriteLine(response);