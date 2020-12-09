$location="northeurope"
$resourceGroupName="advent-fest-demo"
$systemName="hotdognothotdog"

# Generate a unique suffix for the service name
$randomNum=Get-Random -Maximum 1000 -Minimum 100
$serviceName=$systemName + $randomNum
Write-Output $serviceName


Write-Output "### Creating Resource Group"
az group create -n $resourceGroupName -l $location

Write-Output "### Creating Azure SignalR"
az signalr create --name $serviceName --resource-group $resourceGroupName --sku "Free_F1"
$signalRConnString=$(az signalr key list --name $serviceName --resource-group $resourceGroupName --query primaryConnectionString -o tsv)


Write-Output "### Creating Storage Account"
az storage account create -n $serviceName -g $resourceGroupName -l $location --sku Standard_LRS
$storeKey=$(az storage account keys list -n $serviceName -g $resourceGroupName --query "[0].value" -o tsv)
$storageConnString=$(az storage account show-connection-string -n $serviceName -g $resourceGroupName -o tsv)
az storage container create -n "sample-images" --account-name $serviceName --account-key $storeKey --public-access blob


Write-Output "### Creating CosmosDB"
$databaseName="hotdogsphotos"
$containerName="photosclassification"

az cosmosdb create -n $serviceName -g $resourceGroupName
az cosmosdb sql database create -a $serviceName -g $resourceGroupName -n $databaseName
az cosmosdb sql container create -a $serviceName -g $resourceGroupName -d $databaseName -n $containerName -p '/key'
$cosmosConnString=$(az cosmosdb keys list -n $serviceName -g $resourceGroupName --type connection-strings --query "connectionStrings[0].connectionString" -o tsv)


Write-Output "### Connection Strings"
Write-Output "# SignalR"
Write-Output $signalRConnString
Write-Output "# Storage"
Write-Output $storageConnString
Write-Output "# Cosmos"
Write-Output $cosmosConnString