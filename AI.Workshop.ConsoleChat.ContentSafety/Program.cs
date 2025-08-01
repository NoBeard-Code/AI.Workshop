using AI.Workshop.ConsoleChat.ContentSafety;

//var foundryEvaluation = new AzureFoundryEvaluationService();
//await foundryEvaluation.EvaluateContentSafetyAsync("Tell me a joke");

//var evaluation = new AzureContentSafetyService();
//await evaluation.EvaluateContentSafetyAsync();

var evaluation = new AzureContentSafetyWithChatExample();
//await evaluation.EvaluateContentSafetyAsync();
await evaluation.EvaluateWithMiddlewareAsync();