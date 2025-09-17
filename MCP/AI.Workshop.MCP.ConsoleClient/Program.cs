using AI.Workshop.MCP.ConsoleClient;

// NOTE: you must run both MCP client and server app for this to work

var myServer = new McpServerStdioExamples();
await myServer.EnlistServerInfoAsync();
await myServer.CallMcpServerToolsAsync();
await myServer.CallMonkeyToolsAsync();

// NOTE: you must run Ollama app locally for this to work

var ollama = new OllamaIntegrationExamples();
//await ollama.BasicRagWithMcpToolsAsync();
await ollama.RagWithToolsFromGitHubServerAsync();


