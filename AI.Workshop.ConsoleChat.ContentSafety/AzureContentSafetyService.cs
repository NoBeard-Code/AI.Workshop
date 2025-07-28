using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.ContentSafety;

internal class AzureContentSafetyService
{
    private readonly ContentSafetyClient _contentSafetyClient;

    public AzureContentSafetyService()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var contentSafetyEndpoint = config["AZURE_CONTENT_SAFETY_ENDPOINT"];
        var contentSafetyKey = config["AZURE_CONTENT_SAFETY_KEY"];

        _contentSafetyClient = new ContentSafetyClient(
            new Uri(contentSafetyEndpoint), new AzureKeyCredential(contentSafetyKey));
    }

    internal async Task EvaluateContentSafetyAsync()
    {
        while (true)
        {
            Console.Write("Q: ");
            var query = Console.ReadLine();

            var request = new AnalyzeTextOptions(query);

            var response = await _contentSafetyClient.AnalyzeTextAsync(request);

            if (response.Value.CategoriesAnalysis.Any(x => x.Severity > 0))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Content is NOT safe!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Content is considered safe.");
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
        }
    }
}
