namespace AI.Workshop.VectorStore.Ingestion;

public interface IIngestedDocument<TKey>
    where TKey : notnull
{
    TKey Key { get; set; }

    string SourceId { get; set; }

    string DocumentId { get; set; }

    string DocumentVersion { get; set; }
}
