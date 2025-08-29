using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

public class IngestedChunk
{
    private const int VectorDimensions = 384; // 384 is the default vector size for the all-minilm embedding model
    private const string VectorDistanceFunction = DistanceFunction.DotProductSimilarity;

    [VectorStoreKey]
    public required Guid Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    [VectorStoreVector(VectorDimensions, DistanceFunction = VectorDistanceFunction)]
    public string? Vector => Text;
}
