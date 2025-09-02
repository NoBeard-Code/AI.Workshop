namespace AI.Workshop.VectorStore.Ingestion.Qdrant;

public class PDFDirectorySource(string sourceDirectory) : PDFDirectorySource<Guid, IngestedDocument, IngestedChunk>(sourceDirectory)
{
    public override IngestedDocument CreateDocument(string sourceFileId, string sourceFileVersion)
    {
        return new IngestedDocument
        {
            Key = Guid.NewGuid(),
            SourceId = SourceId,
            DocumentId = sourceFileId,
            DocumentVersion = sourceFileVersion
        };
    }

    public override IngestedChunk CreateChunk(string documentId, int pageNumber, string text)
    {
        return new IngestedChunk
        {
            Key = Guid.NewGuid(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            Text = text
        };
    }
}
