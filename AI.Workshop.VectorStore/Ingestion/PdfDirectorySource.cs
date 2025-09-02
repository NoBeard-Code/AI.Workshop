using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace AI.Workshop.VectorStore.Ingestion;

public class PDFDirectorySource(string sourceDirectory) 
    : PDFDirectorySource<string, IngestedDocument, IngestedChunk>(sourceDirectory)
{
    public override IngestedDocument CreateDocument(string sourceFileId, string sourceFileVersion)
    {
        return new IngestedDocument
        {
            Key = Guid.CreateVersion7().ToString(),
            SourceId = SourceId,
            DocumentId = sourceFileId,
            DocumentVersion = sourceFileVersion
        };
    }

    public override IngestedChunk CreateChunk(string documentId, int pageNumber, string text)
    {
        return new IngestedChunk
        {
            Key = Guid.CreateVersion7().ToString(),
            DocumentId = documentId,
            PageNumber = pageNumber,
            Text = text
        };
    }
}

public abstract class PDFDirectorySource<TKey, TDocument, TChunk>(string sourceDirectory) 
    : IIngestionSource<TDocument, TChunk>
    where TKey : notnull
    where TDocument : class, IIngestedDocument<TKey>
    where TChunk : class, IIngestedChunk<TKey>
{
    public static string SourceFileId(string path) => Path.GetFileName(path);
    public static string SourceFileVersion(string path) => File.GetLastWriteTimeUtc(path).ToString("o");

    public string SourceId => $"{nameof(PDFDirectorySource)}:{sourceDirectory}";


    public abstract TDocument CreateDocument(string sourceFileId, string sourceFileVersion);

    public abstract TChunk CreateChunk(string documentId, int pageNumber, string text);

    public Task<IEnumerable<TDocument>> GetNewOrModifiedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments)
    {
        var results = new List<TDocument>();
        var sourceFiles = Directory.GetFiles(sourceDirectory, "*.pdf");
        var existingDocumentsById = existingDocuments.ToDictionary(d => d.DocumentId);

        foreach (var sourceFile in sourceFiles)
        {
            var sourceFileId = SourceFileId(sourceFile);
            var sourceFileVersion = SourceFileVersion(sourceFile);
            var existingDocumentVersion = existingDocumentsById.TryGetValue(sourceFileId, out var existingDocument) ? existingDocument.DocumentVersion : null;
            if (existingDocumentVersion != sourceFileVersion)
            {
                results.Add(CreateDocument(sourceFileId, sourceFileVersion));
            }
        }

        return Task.FromResult((IEnumerable<TDocument>)results);
    }

    public Task<IEnumerable<TDocument>> GetDeletedDocumentsAsync(IReadOnlyList<TDocument> existingDocuments)
    {
        var currentFiles = Directory.GetFiles(sourceDirectory, "*.pdf");
        var currentFileIds = currentFiles.ToLookup(SourceFileId);
        var deletedDocuments = existingDocuments.Where(d => !currentFileIds.Contains(d.DocumentId));
        return Task.FromResult(deletedDocuments.AsEnumerable());
    }

    public Task<IEnumerable<TChunk>> CreateChunksForDocumentAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, TDocument document)
    {
        using var pdf = PdfDocument.Open(Path.Combine(sourceDirectory, document.DocumentId));
        var paragraphs = pdf.GetPages().SelectMany(GetPageParagraphs).ToList();

        return Task.FromResult(paragraphs.Select(p => CreateChunk(document.DocumentId, p.PageNumber, p.Text)));
    }

    private static IEnumerable<(int PageNumber, int IndexOnPage, string Text)> GetPageParagraphs(Page pdfPage)
    {
        var letters = pdfPage.Letters;
        var words = NearestNeighbourWordExtractor.Instance.GetWords(letters);
        var textBlocks = DocstrumBoundingBoxes.Instance.GetBlocks(words);
        var pageText = string.Join(Environment.NewLine + Environment.NewLine,
            textBlocks.Select(t => t.Text.ReplaceLineEndings(" ")));

#pragma warning disable SKEXP0050 // Type is for evaluation purposes only
        return TextChunker.SplitPlainTextParagraphs([pageText], 200)
            .Select((text, index) => (pdfPage.Number, index, text));
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only
    }
}
