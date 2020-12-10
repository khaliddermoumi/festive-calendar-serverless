using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HotDogFunctions
{
    public static class ProcessImage
    {
        [FunctionName("ProcessImage")]
        public static Task Run(
            [BlobTrigger("sample-images/{name}", Connection = "AzureStorageConnectionString")] Stream myBlob, 
            [Blob("sample-images/{name}", FileAccess.ReadWrite, Connection = "AzureStorageConnectionString")] CloudBlockBlob blob,
            [CosmosDB(
                databaseName: "hotdogsphotos",
                collectionName: "photosclassification",
                ConnectionStringSetting = "CosmosDBConnection")]out dynamic document,
            ILogger log)
        {
            log.LogInformation(
                $"C# Blob trigger function Processed blob\n Name:{blob.Name} \n Size: {myBlob.Length} Bytes");

            var projectId = Environment.GetEnvironmentVariable("CustomVisionProjectId", EnvironmentVariableTarget.Process);
            var publishedName = Environment.GetEnvironmentVariable("CustomVisionPublishedName", EnvironmentVariableTarget.Process);
            var endpoint = Environment.GetEnvironmentVariable("CustomVisionEndpoint", EnvironmentVariableTarget.Process);
            var predictionKey = Environment.GetEnvironmentVariable("CustomVisionPredictionKey", EnvironmentVariableTarget.Process);

            var client = AuthenticatePrediction(endpoint, predictionKey);            

            var result = client.ClassifyImage(new Guid(projectId), publishedName,
                myBlob);

            var resultStr = result.Predictions.Select(c => $"{c.TagName}: {c.Probability:P1}").ToList();

            var jsonString = JsonSerializer.Serialize(resultStr);

            document = new { ImageUrl = blob.Uri.ToString(), Scores = jsonString, id = Guid.NewGuid() };

            return Task.CompletedTask;
        }

        private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            var predictionApi = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };

            return predictionApi;
        }
    }
}