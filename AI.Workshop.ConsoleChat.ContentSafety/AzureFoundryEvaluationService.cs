using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.AI.Evaluation.Safety;
using Microsoft.Extensions.Configuration;

namespace AI.Workshop.ConsoleChat.ContentSafety;

internal class AzureFoundryEvaluationService
{
    protected readonly IChatClient _chatClient;

    public AzureFoundryEvaluationService()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var openAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var openAiKey = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

        _chatClient = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey))
            .GetChatClient(deployment)
            .AsIChatClient();
    }

    internal async Task EvaluateContentSafetyAsync(string query)
    {
        List<ChatMessage> history = [];
        history.Add(new ChatMessage(ChatRole.User, query));

        var response = await _chatClient.GetResponseAsync(history);

        var chatConfig = new ChatConfiguration(_chatClient); // this is wrong setup, therefore it won't produce any results!!

        //var safetyConfig = new ContentSafetyServiceConfiguration(
        //    credential: new AzureKeyCredential(openAiEndpoint),
        //    subscriptionId: "<your-subscription-id>",
        //    resourceGroupName: "<your-resource-group>",
        //    projectName: "<your-ai-foundry-project>"
        //);

        var evaluators = new IEvaluator[]
        {
            new HateAndUnfairnessEvaluator(),
            new SelfHarmEvaluator(),
            new SexualEvaluator(),
            new ViolenceEvaluator()
        };

        var reportingConfig = DiskBasedReportingConfiguration.Create(
            storageRootPath: "./EvalReports",
            chatConfiguration: chatConfig, // safetyConfig.ToChatConfiguration()
            evaluators: evaluators,
            enableResponseCaching: true);

        await using var scenarioRun = await reportingConfig.CreateScenarioRunAsync("ContentSafetyCheck");

        var result = await scenarioRun.EvaluateAsync(history, response);

        var metric = result.Get<EvaluationMetric>(HateAndUnfairnessEvaluator.HateAndUnfairnessMetricName);
        var interpretation = metric.Interpretation;

        var hate = result.Get<NumericMetric>(HateAndUnfairnessEvaluator.HateAndUnfairnessMetricName)?.Value ?? 0;
        var selfHarm = result.Get<NumericMetric>(SelfHarmEvaluator.SelfHarmMetricName)?.Value ?? 0;
        var sexual = result.Get<NumericMetric>(SexualEvaluator.SexualMetricName)?.Value ?? 0;
        var violence = result.Get<NumericMetric>(ViolenceEvaluator.ViolenceMetricName)?.Value ?? 0;

        //return new AzureAIContentSafetyResult(hate, selfHarm, sexual, violence);
    }
}
