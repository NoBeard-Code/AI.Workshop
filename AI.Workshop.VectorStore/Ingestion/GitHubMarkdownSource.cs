using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Text;
using Octokit;

namespace AI.Workshop.VectorStore.Ingestion;

public class GitHubMarkdownSource(GitHubClient gitHubClient, string owner, string repo, string path) : IIngestionSource
{
    public string SourceId => "github_markdown";

    public async Task<IEnumerable<IngestedChunk>> CreateChunksForDocumentAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IngestedDocument document)
    {
        var markdownBytes = await gitHubClient.Repository.Content.GetRawContent(owner, repo, document.DocumentId);
        var markdownString = System.Text.Encoding.UTF8.GetString(markdownBytes);
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only
        var chunks = TextChunker.SplitPlainTextParagraphs([markdownString], 400);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only
        var embeddedChunks = await embeddingGenerator.GenerateAndZipAsync(chunks);
        return embeddedChunks.Select(chunk => new IngestedChunk
        {
            Key = Guid.CreateVersion7().ToString(),            
            DocumentId = document.DocumentId,
            Text = chunk.Value,
        });
    }

    public Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IReadOnlyList<IngestedDocument> existingDocuments)
    {
        return Task.FromResult(Enumerable.Empty<IngestedDocument>());   
    }

    public async Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IReadOnlyList<IngestedDocument> existingDocuments)
    {
        var results = new List<IngestedDocument>();
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
                var ingestedDocument = new IngestedDocument
                {
                    Key = document.Path,
                    DocumentId = document.Path,
                    SourceId = SourceId,
                    DocumentVersion = document.Sha,
                };

                results.Add(ingestedDocument);
            }
        }

        return results;
    }
}
