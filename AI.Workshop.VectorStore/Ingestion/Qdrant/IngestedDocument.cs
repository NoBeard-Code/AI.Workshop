using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

public class IngestedDocument : IngestedDocument<Guid>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.DotProductSimilarity)]
    public override ReadOnlyMemory<float> Vector { get; set; } = new ReadOnlyMemory<float>([0, 0]);
}