using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.InMemory;

namespace AI.Workshop.ConsoleChat.OpenAI;

internal class SemanticSearchExamples
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

    private readonly List<CloudService> _cloudServices =
    [
        new() {
                Key = 0,
                Name = "Azure App Service",
                Description = "Host .NET, Java, Node.js, and Python web applications and APIs in a fully managed Azure service. You only need to deploy your code to Azure. Azure takes care of all the infrastructure management like high availability, load balancing, and autoscaling."
        },
        new() {
                Key = 1,
                Name = "Azure Service Bus",
                Description = "A fully managed enterprise message broker supporting both point to point and publish-subscribe integrations. It's ideal for building decoupled applications, queue-based load leveling, or facilitating communication between microservices."
        },
        new() {
                Key = 2,
                Name = "Azure Blob Storage",
                Description = "Azure Blob Storage allows your applications to store and retrieve files in the cloud. Azure Storage is highly scalable to store massive amounts of data and data is stored redundantly to ensure high availability."
        },
        new() {
                Key = 3,
                Name = "Microsoft Entra ID",
                Description = "Manage user identities and control access to your apps, data, and resources."
        },
        new() {
                Key = 4,
                Name = "Azure Key Vault",
                Description = "Store and access application secrets like connection strings and API keys in an encrypted vault with restricted access to make sure your secrets and your application aren't compromised."
        },
        new() {
                Key = 5,
                Name = "Azure AI Search",
                Description = "Information retrieval at scale for traditional and conversational search applications, with security and options for AI enrichment and vectorization."
        }
    ];


    public SemanticSearchExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var endpoint = config["AZURE_OPENAI_ENDPOINT"];
        var key = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_EMBEDDING_DEPLOYMENT"];

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

        var embeddingClient = client.GetEmbeddingClient(deployment);

        _generator = embeddingClient.AsIEmbeddingGenerator();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-vector-search-app?pivots=azure-openai"/>
    /// <returns></returns>
    internal async Task VectorSearch()
    {
        // Create and populate a vector store with the cloud service data.
        // Use the IEmbeddingGenerator implementation to create and assign an embedding vector
        // for each record in the cloud service data:

        var vectorStore = new InMemoryVectorStore();
        var cloudServicesStore = vectorStore.GetCollection<int, CloudService>("cloudServices");
        await cloudServicesStore.EnsureCollectionExistsAsync();

        foreach (var service in _cloudServices)
        {
            service.Vector = await _generator.GenerateVectorAsync(service.Description);
            await cloudServicesStore.UpsertAsync(service);
        }

        // Create an embedding for a search query and use it to perform a vector search on the vector store:

        string query = "Which Azure service should I use to store my Word documents?";
        var queryEmbedding = await _generator.GenerateVectorAsync(query);

        var results = cloudServicesStore.SearchAsync(queryEmbedding, top: 1);

        // The app prints out the top result of the vector search,
        // which is the cloud service that's most relevant to the original query:

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
        }

        queryEmbedding = await _generator.GenerateVectorAsync("Which Azure service can I use to host my web applications?");

        results = cloudServicesStore.SearchAsync(queryEmbedding, top: 1);

        await foreach (var result in results)
        {
            Console.WriteLine($"Name: {result.Record.Name}");
            Console.WriteLine($"Description: {result.Record.Description}");
            Console.WriteLine($"Vector match score: {result.Score}");
        }
    }
}
