using AI.Workshop.ConsoleChat.OpenAI;

//var config = new ConfigurationBuilder()
//    .AddUserSecrets<Program>()
//    .Build();

//var endpoint = config["AZURE_OPENAI_ENDPOINT"];
//var key = config["AZURE_OPENAI_KEY"];
//var deployment = config["AZURE_OPENAI_DEPLOYMENT"];

//var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key))
//    .GetChatClient(deployment)
//    .AsIChatClient();

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

