using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AI.Workshop.ConsoleChat.RAG;

internal class AzureSearchExamples
{
    private readonly AzureOpenAIClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    protected readonly IChatClient _chatClient;
    private readonly SearchIndexClient _searchIndexClient;

    private Func<string, object, Task<HttpResponseMessage>> _getHttpResponseTask;

    public AzureSearchExamples()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .AddJsonFile("Prompts.json", false, false)
            .Build();

        var openAiEndpoint = config["AZURE_OPENAI_ENDPOINT"];
        var openAiKey = config["AZURE_OPENAI_KEY"];
        var deployment = config["AZURE_EMBEDDING_DEPLOYMENT"];
        var searchEndpoint = config["AZURE_SEARCH_ENDPOINT"];
        var searchKey = config["AZURE_SEARCH_KEY"];

        _client = new AzureOpenAIClient(new Uri(openAiEndpoint), new AzureKeyCredential(openAiKey));

        var embeddingClient = _client.GetEmbeddingClient(deployment);

        _generator = embeddingClient.AsIEmbeddingGenerator();

        _searchIndexClient = new SearchIndexClient(new Uri(searchEndpoint), new AzureKeyCredential(searchKey));

        _getHttpResponseTask = async (indexName, payload) =>
        {
            var apiVersion = config["AZURE_SEARCH_API_VERSION"] ?? "2024-07-01";
            var httpUri = $"{searchEndpoint}/indexes/{indexName}/docs/search?api-version={apiVersion}";

            using var request = new HttpRequestMessage(HttpMethod.Post, httpUri);

            request.Headers.Add("api-key", searchKey);
            request.Content = JsonContent.Create(
                payload,
                options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            var client = new HttpClient();
            return await client.SendAsync(request);
        };
    }

    internal async Task GenerateEmbeddingForQueryAsync(string text)
    {
        var embedding = await _generator.GenerateAsync(text);

        Console.WriteLine($"Embedding dimension: {embedding.Vector.Length}");
        Console.WriteLine($"First 10 values: {string.Join(", ", embedding.Vector.ToArray().Take(10))}");

        ReadOnlyMemory<float> vector = await _generator.GenerateVectorAsync(text);

        vector.ToArray().AsSpan(0, 5).ToArray().ToList()
            .ForEach(value => Console.WriteLine($"Vector value: {value}"));
    }

    internal async Task SearchIndexViaHtmlAsync(string text, string indexName)
    {
        var embedding = await _generator.GenerateAsync(text);

        var embeddingResult = embedding.Vector.ToArray();

        var query = new
        {
            QueryText = text,
            Vector = embeddingResult,
            IndexName = indexName,
            TopK = 5,
            VectorBoost = 1.0f
        };

        var payload = new
        {
            count = true,
            search = query.QueryText,
            select = "id,title,searchable_content,summary,url,date,content_type,tags,word_count",
            top = query.TopK,
            vectorQueries = new[]
            {
                new
                {
                    vector = query.Vector,
                    fields = "embedding",
                    k = query.TopK,
                    kind = "vector",
                    exhaustive = true,
                    weight = query.VectorBoost
                }
            }
        };

        using var response = await _getHttpResponseTask(indexName, payload);
        response.EnsureSuccessStatusCode();
        using var doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync()
        );

        var results = doc.RootElement
            .GetProperty("value")
            .EnumerateArray()
            .Select(hit => new
            {
                Id = hit.GetProperty("id").GetString(),
                Title = hit.GetProperty("title").GetString(),
                Content = hit.GetProperty("searchable_content").GetString(),
                Summary = GetOptionalStringProperty(hit, "summary"),
                Url = GetOptionalStringProperty(hit, "url"),
                Date = GetOptionalStringProperty(hit, "date"),
                ContentType = GetOptionalStringProperty(hit, "content_type"),
                WordCount = GetOptionalIntProperty(hit, "word_count"),
                Tags = GetOptionalStringArrayProperty(hit, "tags"),
                Score = hit.GetProperty("@search.score").GetDouble()
            }).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("Context from documents in the database:\n");

        var idx = 1;
        foreach (var r in results)
        {
            sb.AppendLine($"--- Document {idx} ---");
            sb.AppendLine($"**Title:** {r.Title}");
            sb.AppendLine($"**Chunk ID:** {r.Id}");
            sb.AppendLine($"**Content type:** {r.ContentType}");
            sb.AppendLine($"**Tags:** {r.Tags}");
            sb.AppendLine($"**Date:** {r.Date}");
            sb.AppendLine($"**Url:** {r.Url}");
            sb.AppendLine($"**Score:** {r.Score:F3}");
            sb.AppendLine();
            sb.AppendLine(r.Content);
            sb.AppendLine();
            idx++;
        }

        sb.AppendLine("--- User Query ---");
        sb.AppendLine();
        sb.AppendLine(text);

        Console.WriteLine(sb.ToString());
    }

    private static string? GetOptionalStringProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.GetString()
            : null;
    }

    private static int? GetOptionalIntProperty(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
            ? property.GetInt32()
            : null;
    }

    private static string?[] GetOptionalStringArrayProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Array)
        {
            return property.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .ToArray();
        }
        return Array.Empty<string>();
    }

    internal async Task SearchVectorIndexAsync(string query)
    {
        var embedding = await _generator.GenerateAsync(query);

        var embeddingResult = embedding.Vector.ToArray();

        var searchQuery = new
        {
            QueryText = query,
            Vector = embeddingResult,
            IndexName = "inhalt-index"
        };

        var searchClient = _searchIndexClient.GetSearchClient("inhalt-index");

    }

}
