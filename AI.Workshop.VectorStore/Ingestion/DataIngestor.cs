using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion;

public class DataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<string, IngestedChunk> chunksCollection,
    VectorStoreCollection<string, IngestedDocument> documentsCollection) 
    : DataIngestor<string, IngestedDocument, IngestedChunk>(embeddingGenerator, chunksCollection, documentsCollection)
{
}

public abstract class DataIngestor<TKey, TDocument, TChunk>(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<TKey, TChunk> chunksCollection,
    VectorStoreCollection<TKey, TDocument> documentsCollection) 
    where TKey : notnull
    where TDocument : class, IIngestedDocument<TKey>
    where TChunk : class, IIngestedChunk<TKey>
{
    public VectorStoreCollection<TKey, TChunk> Chunks => chunksCollection;

    public static async Task IngestDataAsync(IServiceProvider services, IIngestionSource<TDocument, TChunk> source)
    {
        using var scope = services.CreateScope();
        var ingestor = scope.ServiceProvider.GetRequiredService<DataIngestor<TKey, TDocument, TChunk>>();
        await ingestor.IngestDataAsync(source);
    }

    public async Task IngestDataAsync(IIngestionSource<TDocument, TChunk> source)
    {
        await chunksCollection.EnsureCollectionExistsAsync();
        await documentsCollection.EnsureCollectionExistsAsync();

        var sourceId = source.SourceId;
        var documentsForSource = await documentsCollection.GetAsync(doc => doc.SourceId == sourceId, top: int.MaxValue).ToListAsync();

        var deletedDocuments = await source.GetDeletedDocumentsAsync(documentsForSource);
        foreach (var deletedDocument in deletedDocuments)
        {
            Console.WriteLine("Removing ingested data for {0}", deletedDocument.DocumentId);
            await DeleteChunksForDocumentAsync(deletedDocument);
            await documentsCollection.DeleteAsync(deletedDocument.Key);
        }

        var modifiedDocuments = await source.GetNewOrModifiedDocumentsAsync(documentsForSource);
        foreach (var modifiedDocument in modifiedDocuments)
        {
            Console.WriteLine("Processing {0}", modifiedDocument.DocumentId);
            await DeleteChunksForDocumentAsync(modifiedDocument);

            await documentsCollection.UpsertAsync(modifiedDocument);

            var newRecords = await source.CreateChunksForDocumentAsync(embeddingGenerator, modifiedDocument);
            await chunksCollection.UpsertAsync(newRecords);
        }

        Console.WriteLine("Ingestion is up-to-date");

        async Task DeleteChunksForDocumentAsync(TDocument document)
        {
            var documentId = document.DocumentId;
            var chunksToDelete = await chunksCollection.GetAsync(record => record.DocumentId == documentId, int.MaxValue).ToListAsync();
            if (chunksToDelete.Any())
            {
                await chunksCollection.DeleteAsync(chunksToDelete.Select(r => r.Key));
            }
        }
    }
}
