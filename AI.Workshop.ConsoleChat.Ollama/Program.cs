using AI.Workshop.ConsoleChat.Ollama;

Console.WriteLine("Welcome to the AI Workshop Console Chat Ollama examples!\r\n");

//var search = new BasicLocalOllamaExamples();
//await search.BasicPromptWithHistoryAsync();
//await search.BasicLocalStoreSearchAsync();
//await search.BasicRagWithLocalStoreSearchAsync();

//var search = new SqlLiteDocumentSearch();
//await search.BasicDocumentSearchAsync();    

var search = new QdrantDocumentSearch();
//await search.TestQdrantAsync();
//await search.QdrantSetupAsync();
await search.BasicDocumentSearchAsync();
