# Festive Calendar Serverless Workshop

## Due 11th Dec

- Create deployment script using Azure CLI (except Functions) (Joao) - **Done**
- Create data store (Cosmos DB) to store data (Joao) - **Done**
- Connect Cosmos DB with functions to persist data (Joao) - **Done**
- Create Power BI environment/workspace (Hugo)
- Create Power BI Dashboard w/ streaming source (real-time) (Hugo)
- Create Azure Stream Analytics instance + job (Hugo)
- Connect Stream Analytics with Power BI (Hugo)
- Configure dashboard to show data (Power BI) (Hugo)

## Due 13th Dec

- Create Slides (Both)
- Create document with all workshop steps (Both)

## Steps

### Repository preparation

1. Fork the repository
2. Clone locally
3. Run `deploy.ps1`
   1. You need to have the Azure CLI installed
   2. Authenticate using `az login`
   3. The script will create the following:
      1. Azure Resource Group
      2. Azure SignalR service
      3. Azure Storage Account
      4. Azure CosmosDB with Database and Container
      5. Azure Function
4. Copy the generated connection strings at the bottom of the script

### Azure Function Preparation

1. Paste the connection strings in the **HotDogFunctions** `local.settings.json`

    The values on the file should look similar to this:

    ```json
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "CustomVisionEndpoint": "https://foodcustomvision.cognitiveservices.azure.com/",
        "CustomVisionPredictionKey": "74f181dc48934a81937d185908e5c3d4",
        "CustomVisionProjectId": "17d67036-31ed-4e1e-acc6-57e147af7ac0",
        "CustomVisionPublishedName": "HotDogsDetectionModel",
        "AzureSignalRConnectionString": "xxxxx",
        "AzureStorageConnectionString": "xxxxx",
        "CosmosDBConnection": "xxxxx"
        }
    ```

2. Start the Function App and check that it runs
3. Deploy the function **(Visual Studio 2019)**:
   1. In Visual Studio, right click the function app and click publish
   2. Go through the steps and select:
      1. Azure Function Windows
      2. Pick the previously created Azure Function
      3. Select **Publish(generates pubxml file)**
      4. Before publishing select **Manage Azure App Service Settings**
      5. Copy **Connection Strings and Custom Vision setting to Remote**
      6. Click Publish
4. Once it's deployed Copy the Function App URL

### Web Application Preparation

1. In the **HotDogWebApp** folder expand `wwwroot`
2. Open `app.settings.json` and replace the **HotDogApi** setting with the Function App URL

    It should look similar to this

    ```json
    {
    "HotDogApi": "https://hotdognothotdog000.azurewebsites.net/api/"
    }
    ```

3. Start the Web App and check that it runs
4. Deploy the function **(Azure Portal)**:
   1. Create new **Static Web App Preview** resource
   2. Select **Resource Group, Name and Region**
   3. Authorize **GitHub**
   4. Select the **festive-calendar-serverless** repository
   5. Select **Blazor** as the build preset
   6. Change **App Location** to **HotDogWebApp**
   7. Create the website, this will create a `yaml` in your repository and use GitHub actions to trigger a deploy
