using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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

            var result = client.ClassifyImage(new Guid(projectId ?? string.Empty), publishedName,
                myBlob);

            // var resultStr = result.Predictions.Select(c => $"{c.TagName}: {c.Probability}").ToList();

            // var jsonString = JsonSerializer.Serialize(resultStr);
            var dynamicObject = new ExpandoObject() as IDictionary<string, Object>;
            dynamicObject.TryAdd("ImageUrl", blob.Uri.ToString());
            dynamicObject.TryAdd("Id", Guid.NewGuid());


            foreach(var pred in result.Predictions){
                dynamicObject.TryAdd(pred.TagName, pred.Probability);
            }

            document = dynamicObject;

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