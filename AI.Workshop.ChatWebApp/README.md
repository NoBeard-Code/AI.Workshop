# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

# Configure the AI Model Provider
## Using Azure OpenAI

To use Azure OpenAI, you will need an Azure account and an Azure OpenAI Service resource. For detailed instructions, see the [Azure OpenAI Service documentation](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource).

### 1. Create an Azure OpenAI Service Resource
[Create an Azure OpenAI Service resource](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource?pivots=web-portal).

### 2. Deploy the Models
Deploy the `gpt-4o-mini` and `text-embedding-3-small` models to your Azure OpenAI Service resource. When creating those deployments, give them the same names as the models (`gpt-4o-mini` and `text-embedding-3-small`). See the Azure OpenAI documentation to learn how to [Deploy a model](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource?pivots=web-portal#deploy-a-model).

### 3. Configure API Key and Endpoint
Configure your Azure OpenAI API key and endpoint for this project, using .NET User Secrets:
   1. In the Azure Portal, navigate to your Azure OpenAI resource.
   2. Copy the "Endpoint" URL and "Key 1" from the "Keys and Endpoint" section.
   3. In Visual Studio, right-click on your project in the Solution Explorer and select "Manage User Secrets".
   4. This will open a secrets.json file where you can store your API key and endpoint without it being tracked in source control. Add the following keys & values to the file:

      ```json
      {
        "AzureOpenAI:Key": "YOUR-AZURE-OPENAI-KEY",
        "AzureOpenAI:Endpoint": "YOUR-AZURE-OPENAI-ENDPOINT"
      }
      ```

Make sure to replace `YOUR-AZURE-OPENAI-KEY` and `YOUR-AZURE-OPENAI-ENDPOINT` with your actual Azure OpenAI key and endpoint. Make sure your endpoint URL is formatted like https://YOUR-DEPLOYMENT-NAME.openai.azure.com/ (do not include any path after .openai.azure.com/).

## Configure Azure AI Search

To use Azure AI Search, you will need an Azure account and an Azure AI Search resource. For detailed instructions, see the [Azure AI Search documentation](https://learn.microsoft.com/azure/search/search-create-service-portal).

### 1. Create an Azure AI Search Resource
Follow the instructions in the [Azure portal](https://portal.azure.com/) to create an Azure AI Search resource. Note that there is a free tier for the service but it is not currently the default setting on the portal.

Note that if you previously used the same Azure AI Search resource with different model using this project name, you may need to delete your `data-ai_workshop_chatwebapp-chunks` and `data-ai_workshop_chatwebapp-documents` indexes using the [Azure portal](https://portal.azure.com/) first before continuing; otherwise, data ingestion may fail due to a vector dimension mismatch.

### 3. Configure API Key and Endpoint
   Configure your Azure AI Search API key and endpoint for this project, using .NET User Secrets:
   1. In the Azure Portal, navigate to your Azure AI Search resource.
   2. Copy the "Endpoint" URL and "Primary admin key" from the "Keys" section.
   3. In Visual Studio, right-click on your project in the Solution Explorer and select "Manage User Secrets".
   4. This will open a `secrets.json` file where you can store your API key and endpoint without them being tracked in source control. Add the following keys and values to the file:

      ```json
      {
        "AzureAISearch:Key": "YOUR-AZURE-AI-SEARCH-KEY",
        "AzureAISearch:Endpoint": "YOUR-AZURE-AI-SEARCH-ENDPOINT"
      }
      ```
Make sure to replace `YOUR-AZURE-AI-SEARCH-KEY` and `YOUR-AZURE-AI-SEARCH-ENDPOINT` with your actual Azure AI Search key and endpoint.


# Running the application

## Using Visual Studio

1. Open the `.csproj` file in Visual Studio.
2. Press `Ctrl+F5` or click the "Start" button in the toolbar to run the project.

## Using Visual Studio Code

1. Open the project folder in Visual Studio Code.
2. Install the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) for Visual Studio Code.
3. Once installed, Open the `Program.cs` file.
4. Run the project by clicking the "Run" button in the Debug view.

# Updating JavaScript dependencies

This template leverages JavaScript libraries to provide essential functionality. These libraries are located in the wwwroot/lib folder. For instructions on updating each dependency, please refer to the README.md file in each respective folder.

# Learn More
To learn more about development with .NET and AI, check out the following links:

* [AI for .NET Developers](https://learn.microsoft.com/dotnet/ai/)
