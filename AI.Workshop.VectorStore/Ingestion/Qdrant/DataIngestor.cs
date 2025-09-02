using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

public class DataIngestor(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStoreCollection<Guid, IngestedChunk> chunksCollection,
    VectorStoreCollection<Guid, IngestedDocument> documentsCollection) 
    : DataIngestor<Guid, IngestedDocument, IngestedChunk>(embeddingGenerator, chunksCollection, documentsCollection)
{
}

