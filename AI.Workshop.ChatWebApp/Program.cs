using Microsoft.Extensions.AI;
using AI.Workshop.ChatWebApp.Components;
using AI.Workshop.ChatWebApp.Services;
using AI.Workshop.ChatWebApp.Services.Ingestion;
using Azure;
using Azure.AI.OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// You will need to set the endpoint and key to your own values
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set AzureOpenAI:Endpoint https://YOUR-DEPLOYMENT-NAME.openai.azure.com
//   dotnet user-secrets set AzureOpenAI:Key YOUR-API-KEY
var azureOpenAi = new AzureOpenAIClient(
    new Uri(builder.Configuration["AzureOpenAI:Endpoint"] ?? throw new InvalidOperationException("Missing configuration: AzureOpenAi:Endpoint. See the README for details.")),
    new ApiKeyCredential(builder.Configuration["AzureOpenAI:Key"] ?? throw new InvalidOperationException("Missing configuration: AzureOpenAi:Key. See the README for details.")));
var chatClient = azureOpenAi.GetChatClient("gpt-4o-mini").AsIChatClient();
var embeddingGenerator = azureOpenAi.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// You will need to set the endpoint and key to your own values
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set AzureAISearch:Endpoint https://YOUR-DEPLOYMENT-NAME.search.windows.net
//   dotnet user-secrets set AzureAISearch:Key YOUR-API-KEY
var azureAISearchEndpoint = new Uri(builder.Configuration["AzureAISearch:Endpoint"]
    ?? throw new InvalidOperationException("Missing configuration: AzureAISearch:Endpoint. See the README for details."));
var azureAISearchCredential = new AzureKeyCredential(builder.Configuration["AzureAISearch:Key"]
    ?? throw new InvalidOperationException("Missing configuration: AzureAISearch:Key. See the README for details."));
builder.Services.AddAzureAISearchCollection<IngestedChunk>("data-ai_workshop_chatwebapp-chunks", azureAISearchEndpoint, azureAISearchCredential);
builder.Services.AddAzureAISearchCollection<IngestedDocument>("data-ai_workshop_chatwebapp-documents", azureAISearchEndpoint, azureAISearchCredential);

builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
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

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();
