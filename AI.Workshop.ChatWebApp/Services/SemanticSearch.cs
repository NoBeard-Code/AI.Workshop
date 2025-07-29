namespace AI.Workshop.ChatWebApp.Services;

public class SemanticSearch
{
    public async Task<IEnumerable<dynamic>> SearchAsync(string text, string? documentIdFilter, int maxResults)
    {
        var results = new[]
        {
            new { DocumentId = "Doc1.pdf", PageNumber = 1, Text = "First page content." },
            new { DocumentId = "Doc1.pdf", PageNumber = 2, Text = "Second page content." },
            new { DocumentId = "Doc2.pdf", PageNumber = 1, Text = "Intro page content." }
        };

        return results;
    }
}
