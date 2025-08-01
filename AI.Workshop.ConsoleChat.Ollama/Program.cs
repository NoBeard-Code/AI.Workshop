using AI.Workshop.ConsoleChat.Ollama;

Console.WriteLine("Welcome to the AI Workshop Console Chat Ollama examples!\r\n");

var basicExamples = new BasicLocalOllamaExamples();
//await basicExamples.BasicPromptAsync();
await basicExamples.BasicPromptWithSystemMessageAsync();
