using AI.Workshop.ConsoleChat.RAG;

Console.WriteLine("Welcome to the AI Workshop Console Chat RAG examples!\r\n");

var search = new InMemoryVectorStoreSearch();

await search.GenerateVectorsAsync();
await search.SearchAsync("Which Azure service should I use to store my Word documents?");

var userPrompt = "I'm testing. Fetch me one article from your knowledge base, a seminar from inhalt index and tell me the current time.";

//var azSearch = new AzureSearchExamples();

//await azSearch.GenerateEmbeddingForQueryAsync(userPrompt);
//await azSearch.SearchVectorIndexAsync(userPrompt);

//await azSearch.SearchIndexViaHtmlAsync(userPrompt, "inhalt-index");
//await azSearch.SearchIndexViaHtmlAsync(userPrompt, "knowledge-base");
//await azSearch.SearchIndexAsync(userPrompt, "inhalt-index");
//await azSearch.SearchIndexAsync(userPrompt, "knowledge-base");
//await azSearch.SearchIndexTypedAsync(userPrompt, "inhalt-index");
//await azSearch.SearchIndexTypedAsync(userPrompt, "knowledge-base");

//var tools = new BasicToolsExamples();

//await tools.ItemPriceMethod();
//await tools.ShoppingCartMethods();

var workflow = new RagWorkflowExamples();

//await workflow.InitialMessageLoopAsync();
//await workflow.RagWithBasicToolAsync();
//await workflow.RagWithToolDefinitionsAsync();
await workflow.RagWithSearchToolsByDefaultAsync(userPrompt);
//await workflow.RagWithSearchToolsCustomizedAsync(userPrompt);

