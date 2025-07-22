using AI.Workshop.ConsoleChat.OpenAI;

Console.WriteLine("Welcome to the AI Workshop Console Chat OpenAI examples!\r\n");

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

/*
var stateful = new StatefulServiceExamples();

// 1) Stateless prompts example:
await stateful.SendStatelessPrompts();

// 2) Stateful prompts example:
await stateful.SendStatefulPromptsExplicitly();

// 3) Implicitly stateful prompts example:
await stateful.SendStatefulPromptsImplicitly();
*/

/*
var embeddings = new EmbeddingExamples();

// 1) Generate embeddings for a single text:
await embeddings.GenerateEmbeddingsForSingleInput();

// 2) Generate embeddings for multiple texts:
await embeddings.GenerateEmbeddingsForMultipleInputs();

// 3) Generate a vector from a text:
await embeddings.GenerateVectorFromInput();

// 4) Use a delegating embedding generator with rate limiting:
await embeddings.UseDelegatingEmbeddingGenerator();
*/

var more = new MoreChatExamples();

// 1) Hiking chat example:
//await more.HikingChat();

// 2) Sentiment analysis example:
//await more.RequestStructuredOutput();

// 3) Sentiment analysis for multiple inputs:
await more.SentimentAnalysisForMoreInputs();


