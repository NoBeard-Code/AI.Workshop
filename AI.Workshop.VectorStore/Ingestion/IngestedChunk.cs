using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore.Ingestion;

public class IngestedChunk : IngestedChunk<string>
{
    [VectorStoreVector(VectorDimensions, DistanceFunction = DistanceFunction.CosineDistance)]
    public override string? Vector => base.Vector;
}

public abstract class IngestedChunk<TKey> where TKey : notnull
{
    protected const int VectorDimensions = 384; // 384 is the default vector size for the all-minilm embedding model

    [VectorStoreKey]
    public required TKey Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public required string DocumentId { get; set; }

    [VectorStoreData]
    public int PageNumber { get; set; }

    [VectorStoreData]
    public required string Text { get; set; }

    public virtual string? Vector => Text;
}