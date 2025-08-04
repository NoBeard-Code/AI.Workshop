using AI.Workshop.ConsoleChat.Ollama;

Console.WriteLine("Welcome to the AI Workshop Console Chat Ollama examples!\r\n");

var ollama = new BasicLocalOllamaExamples();
//await ollama.BasicPromptWithHistoryAsync();
await ollama.BasicLocalStoreSearchAsync();

