using AI.Workshop.ChatApp.Web.Components;
using AI.Workshop.VectorStore.Ingestion;
using Microsoft.Extensions.AI;
using Octokit;

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

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";

builder.Services.AddSqliteCollection<string, IngestedChunk>("data-ai_workshop_chatapp-chunks", vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedDocument>("data-ai_workshop_chatapp-documents", vectorStoreConnectionString);

//builder.AddQdrantClient("vector-db");
//builder.Services.AddSingleton<IIngestionSource, PDFDirectorySource>();

builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();

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

//await DataIngestor.IngestDataAsync(
//    app.Services,
//    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

var gitHubKey = builder.Configuration["GITHUB_APIKEY"];
var gitHubClient = new GitHubClient(new ProductHeaderValue("AI-Workshop.ChatApp.Web"))
{
    Credentials = new Credentials(gitHubKey)
};

await DataIngestor.IngestDataAsync(
    app.Services,
    new GitHubMarkdownSource(gitHubClient, "dedalusmax", "dice-and-roll2", "/"));

app.Run();
