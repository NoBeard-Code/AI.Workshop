using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace AI.Workshop.ConsoleChat.RAG.Tools;

internal abstract class AzureSearchToolBase
{
    private protected readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private protected readonly SearchIndexClient _searchIndexClient;
    private protected readonly string _query;

    protected AzureSearchToolBase(AzureOpenAIClient client, IConfigurationRoot config, string query)
    {
        _query = query;

        var deployment = config["AZURE_EMBEDDING_DEPLOYMENT"];
        var searchEndpoint = config["AZURE_SEARCH_ENDPOINT"];
        var searchKey = config["AZURE_SEARCH_KEY"];

        var embeddingClient = client.GetEmbeddingClient(deployment);

        _generator = embeddingClient.AsIEmbeddingGenerator();

        _searchIndexClient = new SearchIndexClient(new Uri(searchEndpoint), new AzureKeyCredential(searchKey));
    }

    public async Task<string> InvokeAsync(string query, int top, string indexName, CancellationToken ct = default)
    {
        query = _query;

        var embedding = await _generator.GenerateAsync(query, cancellationToken: ct);

        var embeddingResult = embedding.Vector.ToArray();

        var searchQuery = new
        {
            QueryText = query,
            Vector = embeddingResult,
            IndexName = indexName,
            TopK = top,
            VectorBoost = 1.0f
        };

        var searchClient = _searchIndexClient.GetSearchClient(searchQuery.IndexName);

        var searchOptions = new SearchOptions
        {
            Size = searchQuery.TopK,
            Select = { "id", "title", "searchable_content", "summary", "url", "date", "content_type", "tags", "word_count" },
            IncludeTotalCount = true,
        };

        searchOptions.VectorSearch = new()
        {
            Queries = {
                new VectorizedQuery(searchQuery.Vector)
                {
                    Fields = { "embedding" },
                    KNearestNeighborsCount = searchQuery.TopK,
                    Weight = searchQuery.VectorBoost,
                    Exhaustive = true,
                }
            }
        };

        SearchResults<IndexSearchResult> response =
            await searchClient.SearchAsync<IndexSearchResult>(searchQuery.QueryText, searchOptions, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Context from documents in the database:\n");

        var idx = 1;
        await foreach (var result in response.GetResultsAsync())
        {
            sb.AppendLine($"--- Document {idx} ---");
            sb.AppendLine($"**Title:** {result.Document.Title}");
            sb.AppendLine($"**ID:** {result.Document.Id}");
            sb.AppendLine($"**Content type:** {result.Document.ContentType}");
            sb.AppendLine($"**Tags:** {result.Document.Tags}");
            sb.AppendLine($"**Url:** {result.Document.Url}");
            sb.AppendLine($"**Score:** {result.Score:F3}");
            sb.AppendLine();
            sb.AppendLine($"{result.Document.Content}");
            sb.AppendLine();
            idx++;
        }

        sb.AppendLine("--- User Query ---");
        sb.AppendLine();
        sb.AppendLine(query);

        return sb.ToString();
    }
}
