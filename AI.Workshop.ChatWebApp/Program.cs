using AI.Workshop.ChatWebApp.Components;
using AI.Workshop.ChatWebApp.Services;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var openAiEndpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
var openAiKey = builder.Configuration["AZURE_OPENAI_KEY"];
var deployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT"];
var embeddingDeployment = builder.Configuration["AZURE_EMBEDDING_DEPLOYMENT"];
var searchEndpoint = builder.Configuration["AZURE_SEARCH_ENDPOINT"];
var searchKey = builder.Configuration["AZURE_SEARCH_KEY"];

var azureOpenAi = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiKey));

var chatClient = azureOpenAi
            .GetChatClient(deployment)
            .AsIChatClient();

var embeddingGenerator = azureOpenAi.GetEmbeddingClient(embeddingDeployment)
    .AsIEmbeddingGenerator();

builder.Services.AddSingleton<SemanticSearch>();

builder.Services.AddChatClient(chatClient)
    .UseFunctionInvocation()
    .UseLogging();

builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
