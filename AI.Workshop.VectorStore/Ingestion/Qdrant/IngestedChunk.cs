using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

public class IngestedChunk : IngestedChunk<Guid>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.DotProductSimilarity)]
    public override string? Vector => base.Vector;
}
