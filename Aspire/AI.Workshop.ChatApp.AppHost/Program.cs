var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama")
    .WithDataVolume();
var chat = ollama.AddModel("chat", "llama3.2");
var embeddings = ollama.AddModel("embeddings", "all-minilm");

var qdrant = builder.AddQdrant("vector-db")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

builder.AddProject<Projects.AI_Workshop_ChatApp_Web>("aichatweb-app")
    .WithReference(chat)
    .WithReference(embeddings)
    .WaitFor(chat)
    .WaitFor(embeddings)
    .WithReference(qdrant)
    .WaitFor(qdrant);

builder.Build().Run();
