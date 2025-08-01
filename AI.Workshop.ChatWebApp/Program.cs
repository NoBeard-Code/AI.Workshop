using AI.Workshop.ChatWebApp.Components;
using AI.Workshop.ChatWebApp.Middleware;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.ClientModel;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Prompts.json", false, false);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var openAiEndpoint = builder.Configuration["AZURE_OPENAI_ENDPOINT"];
var openAiKey = builder.Configuration["AZURE_OPENAI_KEY"];
var deployment = builder.Configuration["AZURE_OPENAI_DEPLOYMENT"];
var embeddingDeployment = builder.Configuration["AZURE_EMBEDDING_DEPLOYMENT"];

var azureOpenAi = new AzureOpenAIClient(new Uri(openAiEndpoint), new ApiKeyCredential(openAiKey));

var chatClient = azureOpenAi
            .GetChatClient(deployment)
            .AsIChatClient();

var embeddingGenerator = azureOpenAi.GetEmbeddingClient(embeddingDeployment)
    .AsIEmbeddingGenerator();

var cache = new MemoryDistributedCache(
    Options.Create(new MemoryDistributedCacheOptions())
    );

var limiter = new ConcurrencyLimiter(new()
{
    PermitLimit = 1, // Limit to one concurrent request
    QueueLimit = int.MaxValue // No limit on the queue size
});

builder.Services.AddChatClient(chatClient)
    .UseFunctionInvocation()
    .UseDistributedCache(cache)
    .UseRateLimiting(limiter)
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
