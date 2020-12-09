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
        private const string Endpoint = "https://foodcustomvision.cognitiveservices.azure.com/";
        private const string TrainingKey = "dcbea48f2c594c93adece1c63ff7cdaf";
        private const string PredictionKey = "74f181dc48934a81937d185908e5c3d4";
        private const string ProjectId = "17d67036-31ed-4e1e-acc6-57e147af7ac0";
        private const string PublishedName = "HotDogsDetectionModel";

        [FunctionName("ProcessImage")]
        public static Task Run(
            [BlobTrigger("sample-images/{name}", Connection = "AzureStorage:ConnectionString")] Stream myBlob, 
            [Blob("sample-images/{name}", FileAccess.ReadWrite, Connection = "AzureStorage:ConnectionString")] CloudBlockBlob blob,
            [CosmosDB(
                databaseName: "hotdogsphotos",
                collectionName: "photosclassification",
                ConnectionStringSetting = "CosmosDBConnection")]out dynamic document,
            ILogger log)
        {
            log.LogInformation(
                $"C# Blob trigger function Processed blob\n Name:{blob.Name} \n Size: {myBlob.Length} Bytes");

            var client = AuthenticatePrediction(Endpoint, PredictionKey);
            
            var result = client.ClassifyImage(new Guid(ProjectId), PublishedName,
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