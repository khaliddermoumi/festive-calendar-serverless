using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;

namespace HotDogFunctions
{
    public static class ProcessImage
    {
        private const string Endpoint = "https://foodcustomvision.cognitiveservices.azure.com/";
        private const string TrainingKey = "dcbea48f2c594c93adece1c63ff7cdaf";
        private const string PredictionKey = "74f181dc48934a81937d185908e5c3d4";

        [FunctionName("ProcessImage")]
        public static void Run([BlobTrigger("sample-images/{name}", Connection = "AzureStorage:ConnectionString")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var client = AuthenticatePrediction(Endpoint, PredictionKey);

            var result = client.ClassifyImage(new Guid("17d67036-31ed-4e1e-acc6-57e147af7ac0"), "HotDogsDetectionModel", myBlob);

            foreach (var c in result.Predictions)
            {
                log.LogInformation($"\t{c.TagName}: {c.Probability:P1}");
            }
        }

        private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create a prediction endpoint, passing in the obtained prediction key
            var predictionApi = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };

            return predictionApi;
        }

    }
}
