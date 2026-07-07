# GenAiChatbot — Setup Instructions

A minimal .NET 10 console chatbot built on **Microsoft.Extensions.AI** and **Azure OpenAI**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed
- An Azure subscription
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed (for `az login` and resource creation)

## 1. Create an Azure OpenAI resource + model deployment

If you don't already have one, run:

```bash
# Log in
az login

# Variables (edit these)
RESOURCE_GROUP="genai-learning-rg"
OPENAI_SERVICE_NAME="genai-learning-openai"
LOCATION="eastus"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Azure OpenAI resource
az cognitiveservices account create \
  --name $OPENAI_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --custom-domain $OPENAI_SERVICE_NAME \
  --kind OpenAI \
  --sku s0

# Deploy a model (gpt-4o-mini is cheap and good for learning)
az cognitiveservices account deployment create \
  --name $OPENAI_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --deployment-name gpt-4o-mini \
  --model-name gpt-4o-mini \
  --model-version 2024-07-18 \
  --model-format OpenAI \
  --sku-name Standard \
  --sku-capacity 1

# Grant yourself permission to call the model (Entra ID auth)
az role assignment create \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --role "Cognitive Services OpenAI User" \
  --scope /subscriptions/$(az account show --query id -o tsv)/resourceGroups/$RESOURCE_GROUP
```

Note your **endpoint** (shown in the Azure Portal under the resource's "Keys and Endpoint" page) and your **deployment name** (`gpt-4o-mini` above).

## 2. Restore packages

From the `GenAiChatbot` folder:

```bash
dotnet restore
```

## 3. Configure secrets

The app reads config from .NET User Secrets, so nothing sensitive is ever committed to source control.

```bash
dotnet user-secrets init
dotnet user-secrets set AZURE_OPENAI_ENDPOINT "https://your-resource.openai.azure.com/"
dotnet user-secrets set AZURE_OPENAI_GPT_NAME "gpt-4o-mini"
```

## 4. Authenticate

The app uses `DefaultAzureCredential` (Entra ID), so just make sure you're logged in via the CLI:

```bash
az login
```

(No API keys to manage or leak — this is the recommended approach over key-based auth.)

## 5. Run it

```bash
dotnet run
```

You should see:

```
Chatbot ready. Type 'exit' to quit.

You: hello
AI: Hi there! How can I help you today?
```

## Troubleshooting

- **"Missing configuration" message on startup** → you skipped step 3; re-run the `dotnet user-secrets set` commands.
- **401/403 errors** → run `az login` again, or double-check the role assignment in step 1 was applied to the right account/subscription.
- **Model not found** → confirm your deployment name in Azure Portal matches exactly what you set for `AZURE_OPENAI_GPT_NAME`.

## Next steps to extend this project

1. **Streaming responses** — replace `GetResponseAsync` with `GetStreamingResponseAsync` and print tokens as they arrive.
2. **Function/tool calling** — give the model a C# method (e.g. `GetWeather(string city)`) it can invoke.
3. **RAG (chat with your documents)** — add an embeddings step and a vector store so the bot can answer from your own files instead of general knowledge.
