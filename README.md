Azure RAG Demo (OpenAiRagApp)
================================

Summary
-------
This repository is a small .NET console application that demonstrates a retrieval-augmented generation (RAG) pipeline using Azure OpenAI for embeddings/chat and Azure Cognitive Search for semantic/vector search. The app supports three modes:

- `Chat` — chat directly with Azure OpenAI (no RAG)
- `Search` — run semantic/vector search against the Search index and inspect retrieved documents
- `RAG` — run a full RAG flow: semantic search + use retrieved content when asking the model

Prerequisites
-------------
- .NET 10 SDK installed
- Git
- An Azure subscription with permission to create resources
- Basic familiarity with Azure Portal / Azure CLI

Quick clone
-----------
1. Clone the repo:

   ```bash
   git clone https://github.com/deepaksnath/AzureOpenAi.git
   cd AzureOpenAi/OpenAiRagApp
   ```

2. Restore/build:

   ```bash
   dotnet build
   ```

Configure the application
-------------------------
The app reads configuration from `appsettings.json` at the project root. Before running, update the `AzureAiSettings` section with your Azure resource values.

Fields to set in `appsettings.json` (example keys shown in the file must be replaced):

- `AzureOpenAiEndpoint` — Azure OpenAI resource endpoint (e.g. `https://YOUR-OPENAI-RESOURCE.openai.azure.com/`)
- `AzureOpenAiApiKey` — key for your Azure OpenAI resource
- `ChatModelDeployment` — name of the chat model deployment you created (e.g. `chat-5.4-nano` or any deployed chat model)
- `EmbeddingModelDeployment` — name of the embedding model deployment (e.g. `embedding-3-small`)
- `SearchEndpoint` — Azure Cognitive Search endpoint (e.g. `https://YOUR-SEARCH.search.windows.net`)
- `SearchApiKey` — admin key for your Cognitive Search service
- `SearchIndexName` — the name of the index used by the app (default in repo: `demo-chat-index`)

Important: do not commit secrets to source control. Use local secrets or environment variables in real projects.

Azure resources to create
-------------------------
At minimum you must create two Azure resources:

1. Azure OpenAI resource
   - Create an Azure OpenAI resource in the portal.
   - Deploy a chat model (e.g. `gpt-4o-mini`, `gpt-4o`, or a model your subscription allows). Give the deployment a name and use that name in `ChatModelDeployment`.
   - Deploy an embeddings model (for example `text-embedding-3-small` or `embedding-3-small`) and use that deployment name in `EmbeddingModelDeployment`.

2. Azure Cognitive Search service
   - Create an Azure Cognitive Search resource.
   - Create an index for vector search. The index used by this app must contain at least:
     - `id` (Edm.String) — key
     - `content` (Edm.String) — retrievable content
     - `vector` (Collection(Edm.Single)) — a vector field to store embeddings
   - Enable vector search on the index/service and ensure the vector field name matches `vector`.

Sample index definition (JSON) — you can use the portal, REST, or `az` CLI to create it. Adapt names as required:

```json
{
  "name": "demo-chat-index",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true, "searchable": false },
    { "name": "content", "type": "Edm.String", "searchable": true, "retrievable": true },
    { "name": "vector", "type": "Collection(Edm.Single)" }
  ],
  "vectorSearch": { "algorithmConfigurations": [ { "name": "vector-config", "kind": "hnsw" } ] }
}
```

Seeding (upload example documents)
----------------------------------
The app can upload a small set of sample documents into the configured index. Use the "seeding" flag to trigger upload when running the app.

From project folder (`OpenAiRagApp`):

- Seed the index (run the `Search` mode with seeding enabled):

  ```bash
  dotnet run -- Search 1
  ```

- Run search mode without seeding:

  ```bash
  dotnet run -- Search 0
  ```

Run modes
---------
From the project folder run with one of the three supported modes and a seeding flag (1 = seed, 0 = don't seed):

- Chat (free chat to Azure OpenAI):
  ```bash
  dotnet run -- Chat 0
  ```

- Search (semantic search only):
  ```bash
  dotnet run -- Search 0
  ```

- RAG (semantic search + ask model):
  ```bash
  dotnet run -- RAG 0
  ```

The app writes messages to the console. Use `1` as the second argument to seed documents before running interactive scenarios.

Debugging locally
-----------------
- Open the project in Visual Studio or Visual Studio Code.
- Set breakpoints in `Program.cs`, `OpenAiApp.cs`, `Services/SemanticSearchService.cs`, or `Services/ChatBotService.cs`.
- Run the app with the desired arguments (see "Run modes").
- Use `dotnet build` to check for compile issues.

Common issues & troubleshooting
-------------------------------
- Authentication / Unauthorized errors
  - Double-check `AzureOpenAiApiKey` and `SearchApiKey` in `appsettings.json`.
  - Ensure you are using an admin key for Cognitive Search when creating indexes or uploading documents.

- Index not found or field missing
  - Confirm `SearchEndpoint` and `SearchIndexName` are correct.
  - Confirm index has a `vector` field of type `Collection(Edm.Single)` and a `content` field.

- Model/Deployment not found
  - Make sure your Azure OpenAI deployments exist and the names in `ChatModelDeployment` / `EmbeddingModelDeployment` match exactly.

- Rate-limiting or quota issues
  - Check your subscription limits and model quotas in the Azure portal.

Security notes
--------------
- Do not check API keys into source control. Replace keys with environment variables or use user secrets for development.
- Consider using managed identities and Azure Key Vault for production deployments.

Helpful commands
----------------
- Build: `dotnet build`
- Run: `dotnet run -- <Chat|Search|RAG> <1|0>`
- Change directory to the project: `cd OpenAiRagApp`

Where to look in the code
-------------------------
- Startup and DI registration: `Extensions/RegisterServices.cs`
- App entry point: `Program.cs`
- App orchestration: `OpenAiApp.cs`
- Semantic search implementation: `Services/SemanticSearchService.cs`
- Chat implementation: `Services/ChatBotService.cs`

If things still fail
--------------------
Collect console logs and the exact exception message, then verify the configuration values in `appsettings.json`. If needed, open an issue in the upstream repo with the exception text and the steps you ran.

License & contribution
----------------------
deepak.s.nath@gmail.com


