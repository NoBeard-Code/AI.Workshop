using AI.Workshop.ConsoleChat.OpenAI;

/*
var basics = new BasicChatExamples();

// 1) the basic usage is to request a chat response:
await basics.HelloPrompt();

// 2) Request a chat response by providing roles:
await basics.RequestChatResponse();

// 3) Request a chat response with history:
await basics.RequestChatResponseWithHistory();

// 4) Request a streaming chat response (using IAsyncEnumerable):
await basics.RequestStreamingChatResponse();

// 5) Request a streaming chat response with history:
await basics.RequestStreamingChatResponseWithHistory();
*/

/*
var builderExamples = new ChatClientBuilderExamples();

// 1) Tool calling example:
await builderExamples.ToolCalling();

// 2) Distributed cache example:
await builderExamples.DistributedCache();

// 3) OpenTelemetry example:
await builderExamples.UseTelemetry();
*/

/*
var middleware = new ChatClientMiddlewareExamples();

// 1) Basic usage with RateLimitingChatClient:
await middleware.BasicUsage();

// 2) Usage with middleware:
await middleware.UsageWithMiddleware();

// 3) Usage without custom class:
await middleware.UsageWithoutCustomClass();
*/

var stateful = new StatefulServiceExamples();

// 1) Stateless prompts example:
//await stateful.SendStatelessPrompts();

// 2) Stateful prompts example:
//await stateful.SendStatefulPromptsExplicitly();

// 3) Implicitly stateful prompts example:
await stateful.SendStatefulPromptsImplicitly();
