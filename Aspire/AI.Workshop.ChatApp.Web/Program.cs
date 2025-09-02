using AI.Workshop.ChatApp.Web.Components;
using AI.Workshop.VectorStore.Ingestion;
using QdrantBased = AI.Workshop.VectorStore.Ingestion.Qdrant;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Octokit;
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddOllamaApiClient("chat")
    .AddChatClient()
    .UseFunctionInvocation()
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());

builder.AddOllamaApiClient("embeddings")
    .AddEmbeddingGenerator();

// Uncomment the following lines to use SQLite as a vector store:

//var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
//var vectorStoreConnectionString = $"Data Source={vectorStorePath}";

//builder.Services.AddSqliteCollection<string, IngestedChunk>("data-ai_workshop_chatapp-chunks", vectorStoreConnectionString);
//builder.Services.AddSqliteCollection<string, IngestedDocument>("data-ai_workshop_chatapp-documents", vectorStoreConnectionString);

// Uncomment the following lines to use Qdrant as a vector store:

builder.AddQdrantClient("vector-db");

//builder.Services.Configure<QdrantVectorStoreOptions>(
//    builder.Configuration.GetSection("Qdrant"));

//builder.Services.AddSingleton<VectorStore>(provider =>
//{
//    var client = provider.GetRequiredService<QdrantClient>();
//    var options = provider.GetRequiredService<IOptions<QdrantVectorStoreOptions>>().Value;

//    return new QdrantVectorStore(client, true, options);
//});

builder.Services.AddQdrantCollection<Guid, QdrantBased.IngestedChunk>("data-ai_workshop_chatapp-chunks");
builder.Services.AddQdrantCollection<Guid, QdrantBased.IngestedDocument>("data-ai_workshop_chatapp-documents");

//builder.Services.AddSingleton<VectorStoreCollection<Guid, IngestedChunk>>();
//builder.Services.AddSingleton<VectorStoreCollection<Guid, IngestedDocument>>();

//builder.Services.AddSingleton(provider =>
//{
//    var store = provider.GetRequiredService<VectorStore>();
//    return store.GetCollection<Guid, IngestedChunk>("data-ai_workshop_chatapp-chunks");
//});

//builder.Services.AddSingleton(provider =>
//{
//    var store = provider.GetRequiredService<VectorStore>();
//    return store.GetCollection<Guid, IngestedDocument>("data-ai_workshop_chatapp-documents");
//});

builder.Services.AddScoped<QdrantBased.DataIngestor>();
builder.Services.AddSingleton<QdrantBased.SemanticSearch>();

var app = builder.Build();

app.MapDefaultEndpoints();

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

using var scope = app.Services.CreateScope();
var ingestor = scope.ServiceProvider.GetRequiredService<QdrantBased.DataIngestor >();

await ingestor.IngestDataAsync(
    new QdrantBased.PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

var gitHubKey = builder.Configuration["GITHUB_APIKEY"];
var gitHubClient = new GitHubClient(new ProductHeaderValue("AI-Workshop.ChatApp.Web"))
{
    Credentials = new Credentials(gitHubKey)
};

//await DataIngestor.IngestDataAsync(
//    app.Services,
//    new GitHubMarkdownSource(gitHubClient, "dedalusmax", "dice-and-roll2", "/"));

app.Run();
