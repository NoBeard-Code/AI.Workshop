using Microsoft.Extensions.AI;

namespace AI.Workshop.VectorStore.Ingestion;

public interface IIngestionSource<TDocument, TChunk> 
    where TDocument : class 
    where TChunk : class
{
    string SourceId { get; }

    Task<IEnumerable<TDocument>> GetNewOrModifiedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments);

    Task<IEnumerable<TDocument>> GetDeletedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments);

    Task<IEnumerable<TChunk>> CreateChunksForDocumentAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, TDocument document);
}