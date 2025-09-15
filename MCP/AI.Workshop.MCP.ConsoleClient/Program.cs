using AI.Workshop.MCP.ConsoleClient;

// NOTE: you must run both MCP client and server app for this to work

var basic = new McpServerStdioExamples();
await basic.EnlistServerInfoAsync();
await basic.CallMcpServerToolsAsync();
await basic.CallMonkeyToolsAsync();

// NOTE: you must run Ollama app locally for this to work

var ollama = new OllamaIntegrationExamples();
await ollama.BasicRagWithMcpToolsAsync();

Console.ReadKey();