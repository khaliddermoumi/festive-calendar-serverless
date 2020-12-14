# Festive Calendar Serverless Workshop

## Agenda
1. Introduction
2. Slides
   1. Overview
   2. Context (Architecture Diagram)
   3. Reference to previous session
3. Workshop
   1. Environment Configuration
   2. Deploy Functions & Apps
   3. Testing
   4. Power BI
4. Wrap-up

## Prerequisites
* Azure Subscription - https://azure.microsoft.com/en-us/free/
* VS Code - https://code.visualstudio.com/Download (in case you prefer to use VS Code)
* Visual Studio 2019 - https://visualstudio.microsoft.com/vs/ (in case you prefer to use Visual Studio)
* Azure CLI - https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
* Azure Functions extension for Visual Studio Code - https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions
* Azure Functions Core Tools - https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#v2
* Git (https://git-scm.com/downloads)

## Steps

### Repository preparation

1. Fork the repository
2. Clone locally
3. Authenticate using `az login`
4. Get the details of the Azure subscription using `az account show`
5. Run `deploy.ps1`
   1. The script will create the following:
      1. Azure Resource Group
      2. Azure SignalR service
      3. Azure Storage Account
      4. Azure CosmosDB with Database and Container
      5. Azure Function
6. Copy the generated connection strings at the bottom of the script

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
3. Deploy the function using **(Visual Studio 2019)**:
   1. In Visual Studio, right click the function app and click publish
   2. Go through the steps and select:
      1. Azure Function Windows
      2. Pick the previously created Azure Function
      3. Select **Publish(generates pubxml file)**
      4. Before publishing select **Manage Azure App Service Settings**
      5. Copy **Connection Strings and Custom Vision setting to Remote**
      6. Click Publish
4. Deploy the function using **VS Code**
   1. 
5. Once it's deployed copy the Function App URL

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

4. Deploy the Web App using **(Azure Portal)**:
   1. Create new **Static Web App Preview** resource
   2. Select **Resource Group, Name and Region**
   3. Authorize **GitHub**
   4. Select the **festive-calendar-serverless** repository
   5. Select **Blazor** as the build preset
   6. Change **App Location** to **HotDogWebApp**
   7. Create the website, this will create a `yaml` in your repository and use GitHub actions to trigger a deploy
5. Deploy the Web App using **VS Code**:
   1. Open Azure Extension
   2. Navigate to Static Web Apps (Preview) section
   3. Create a new *Static Web App*
      1. Right click on your subscription listed in the section
      2. Select *Create Static Web App (Advanced)* option
      3. Select *Use existing Github repository* option
      4. Select your organization
      5. Select the cloned repository
      6. Select *master* branch
      7. Provide a name for your Web App
      8. Select the resource group created previously
      9. Select Blazor as the project type
      10. Select the location
   4.  Modify Workflow file from Github or modify workflow locally by running `git pull` and accessing the workflow file located in the `.github\workflows` folder
       1.  Access your repository on Github
       2.  Select menu option *Actions*
       3.  Select the existing deployment
       4.  Select the three elipses option on the top right corner
       5.  Select *View workflow file* option
       6.  Select the pencil to edit the workflow file
       7.  Modify the *app_location* parameter from `Client` to `HotDogWebApp`
   5.  Navigate to the Azure Portal, locate your Static Web App, and browse the App
   6.  Upload one or more images (with hotdogs or without hotdogs)
   7.  Access the List option (on the left menu) to list the photos uploaded, and ensure your photo is showing up.