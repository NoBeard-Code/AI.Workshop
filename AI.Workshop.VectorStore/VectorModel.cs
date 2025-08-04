using Microsoft.Extensions.VectorData;

namespace AI.Workshop.VectorStore;

public class VectorModel
{
    [VectorStoreKey]
    public int Key { get; set; } // This property will be used as the unique identifier for the vector data in the store.

    [VectorStoreData]
    public string Name { get; set; } // This property will be used to store the name of the vector data.

    [VectorStoreData]
    public string Description { get; set; } // This property will be used to store a description of the vector data.

    [VectorStoreVector(Dimensions: 384, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; } // This property will be used to store the vector representation of the data.
}
