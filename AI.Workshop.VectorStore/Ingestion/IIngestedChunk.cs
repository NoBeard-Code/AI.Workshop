namespace AI.Workshop.VectorStore.Ingestion;

public interface IIngestedChunk<TKey>
    where TKey : notnull
{
    TKey Key { get; set; }

    string DocumentId { get; set; }

    int PageNumber { get; set; }

    string Text { get; set; }
}
