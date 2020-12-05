using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace HotDogFunctions
{
    public static class ProcessImage
    {
        private const string Endpoint = "https://foodcustomvision.cognitiveservices.azure.com/";
        private const string TrainingKey = "dcbea48f2c594c93adece1c63ff7cdaf";
        private const string PredictionKey = "74f181dc48934a81937d185908e5c3d4";

        [FunctionName("ProcessImage")]
        public static Task Run([BlobTrigger("sample-images/{name}", Connection = "AzureStorage:ConnectionString")]
            Stream myBlob, [Blob("sample-images/{name}", FileAccess.ReadWrite,
                Connection = "AzureStorage:ConnectionString")]
            CloudBlockBlob blob, [SignalR(HubName = "HotDogHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformation(
                $"C# Blob trigger function Processed blob\n Name:{blob.Name} \n Size: {myBlob.Length} Bytes");

            var client = AuthenticatePrediction(Endpoint, PredictionKey);

            var result = client.ClassifyImage(new Guid("17d67036-31ed-4e1e-acc6-57e147af7ac0"), "HotDogsDetectionModel",
                myBlob);

            var resultStr = "";
            foreach (var c in result.Predictions)
            {
                log.LogInformation($"\t{c.TagName}: {c.Probability:P1}");

                resultStr += $"{c.TagName}: {c.Probability:P1} \n";
            }

            return signalRMessages.AddAsync(new SignalRMessage
            {
                Target = "clientMessage",
                Arguments = new object[] {new ClientMessage {ImageUrl = blob.Uri.ToString(), Message = resultStr}}
            });
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