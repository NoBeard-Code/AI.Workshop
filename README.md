# 🧠 AI.Workshop

## 📌 Overview
**AI.Workshop** is a collection of sample applications, tools, and demos showcasing how to build AI‑powered solutions with **.NET**, **Blazor**, and **Azure/OpenAI services**.  
It includes:

- Interactive **Blazor** chat UIs
- Console‑based AI demos
- Retrieval‑Augmented Generation (RAG) workflows
- Content safety and moderation examples
- In‑memory and vector store search utilities
- Azure AI Search integration

Whether you’re exploring **prompt engineering**, **semantic search**, or **embedding‑based retrieval**, this workshop provides ready‑to‑run examples and reusable components.

---

## 🏗️ Solution Structure

| Project | Type | Purpose |
|---------|------|---------|
| **AI.Workshop.BlazorChat** | Blazor Server | Minimal chat UI with AI backend |
| **AI.Workshop.ChatWebApp** | Blazor WebAssembly | Rich web chat app with components, middleware, and Azure AI tools |
| **AI.Workshop.ConsoleChat.ContentSafety** | Console App | Demonstrates Azure Content Safety integration |
| **AI.Workshop.ConsoleChat.OpenAI** | Console App | OpenAI/Azure OpenAI chat, embeddings, semantic search |
| **AI.Workshop.ConsoleChat.RAG** | Console App | Retrieval‑Augmented Generation workflows with Azure AI Search |
| **AI.Workshop.VectorStore** | Class Library | In‑memory vector store, ingestion pipelines, semantic search |
| **Aspire/AI.Workshop.ChatApp.AppHost** | App Host | Backend host for Aspire‑based chat app |
| **Aspire/AI.Workshop.ChatApp.Web** | Blazor WebAssembly | Aspire‑based chat UI |

---

## 🚀 Features
- **Blazor Chat UIs** – Modular Razor components for chat, citations, suggestions, and more
- **Azure AI Search Tools** – Indexing, knowledge base queries, and semantic search
- **Content Safety** – Filter and moderate AI responses
- **RAG Examples** – Combine vector search with LLMs for grounded answers
- **Embedding Generation** – Create and query embeddings with Azure OpenAI
- **Rate Limiting Middleware** – Control API usage in chat clients

---

## 🛠️ Tech Stack
- **.NET 8 / C#**
- **Blazor Server & WebAssembly**
- **Azure OpenAI Service**
- **Azure AI Search**
- **TailwindCSS** for styling
- **pdf.js** for PDF rendering in web apps

---

## 📦 Getting Started

### 1️⃣ Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Azure subscription with:
  - Azure OpenAI resource
  - Azure AI Search resource
- Node.js (for TailwindCSS builds, if modifying styles)

### 2️⃣ Clone the Repository
```bash
git clone https://github.com/dedalusmax/AI.Workshop.git
cd AI.Workshop
