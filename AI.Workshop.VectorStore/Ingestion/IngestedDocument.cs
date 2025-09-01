using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion;

public class IngestedDocument : IngestedDocument<string>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.CosineDistance)]
    public override ReadOnlyMemory<float> Vector { get; set; } = new ReadOnlyMemory<float>([0, 0]);
}

public abstract class IngestedDocument<TKey> where TKey : notnull
{
    protected const int VectorDimensions = 2;

    [VectorStoreKey]
    public required TKey Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string SourceId { get; set; }

    [VectorStoreData]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public required string DocumentVersion { get; set; }

    // The vector is not used but required for some vector databases
    public virtual ReadOnlyMemory<float> Vector { get; set; }
}