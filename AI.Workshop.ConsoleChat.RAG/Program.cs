using AI.Workshop.ConsoleChat.RAG;

Console.WriteLine("Welcome to the AI Workshop Console Chat RAG examples!\r\n");

var search = new InMemoryVectorStoreSearch();

await search.GenerateVectorsAsync();
await search.SearchAsync("Which Azure service should I use to store my Word documents?");


