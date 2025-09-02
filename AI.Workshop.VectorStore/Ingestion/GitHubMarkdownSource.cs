using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Text;
using Octokit;

namespace AI.Workshop.VectorStore.Ingestion;

public class GitHubMarkdownSource(GitHubClient gitHubClient, string owner, string repo, string path) 
    : GitHubMarkdownSource<string, IngestedDocument, IngestedChunk>(gitHubClient, owner, repo, path)
{
    public override IngestedDocument CreateDocument(string documentPath, string sourceId, string documentVersion)
    {
        return new IngestedDocument
        {
            Key = documentPath,
            DocumentId = documentPath,
            SourceId = SourceId,
            DocumentVersion = documentVersion
        };
    }

    public override IngestedChunk CreateChunk(string documentId, string text)
    {
        return new IngestedChunk
        {
            Key = Guid.CreateVersion7().ToString(),
            DocumentId = documentId,
            Text = text
        };
    }
}

public abstract class GitHubMarkdownSource<TKey, TDocument, TChunk>(GitHubClient gitHubClient, string owner, string repo, string path) 
    : IIngestionSource<TDocument, TChunk>
    where TKey : notnull
    where TDocument : class, IIngestedDocument<TKey>
    where TChunk : class, IIngestedChunk<TKey>
{
    public string SourceId => "github_markdown";

    public abstract TDocument CreateDocument(string documentPath, string sourceId, string documentVersion);

    public abstract TChunk CreateChunk(string documentId, string text);

    public async Task<IEnumerable<TChunk>> CreateChunksForDocumentAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, TDocument document)
    {
        var markdownBytes = await gitHubClient.Repository.Content.GetRawContent(owner, repo, document.DocumentId);
        var markdownString = System.Text.Encoding.UTF8.GetString(markdownBytes);
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only
        var chunks = TextChunker.SplitPlainTextParagraphs([markdownString], 400);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only
        var embeddedChunks = await embeddingGenerator.GenerateAndZipAsync(chunks);
        return embeddedChunks.Select(chunk => CreateChunk(document.DocumentId, chunk.Value));
    }

    public Task<IEnumerable<TDocument>> GetDeletedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments)
    {
        return Task.FromResult(Enumerable.Empty<TDocument>());   
    }

    public async Task<IEnumerable<TDocument>> GetNewOrModifiedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments)
    {
        var results = new List<TDocument>();
        var allContents = await gitHubClient.Repository.Content.GetAllContents(owner, repo, path);

        foreach (var document in allContents.Where(f => Path.GetExtension(f.Name).Equals(".md", StringComparison.OrdinalIgnoreCase)))
        {
            if (existingDocuments.FirstOrDefault(d => d.DocumentId == document.Path) is { } existingFile)
            {
                if (existingFile.DocumentVersion != document.Sha)
                {
                    existingFile.DocumentVersion = document.Sha;
                    results.Add(existingFile);
                }
            }
            else
            {
                results.Add(CreateDocument(document.Path, SourceId, document.Sha));
            }
        }

        return results;
    }
}
