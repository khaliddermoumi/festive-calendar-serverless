using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace HotDogFunctions
{
    public static class PushMessage
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName("PushMessage")]
        public static void Run([CosmosDBTrigger(
            databaseName: "hotdogsphotos",
            collectionName: "photosclassification",
            ConnectionStringSetting = "CosmosDBConnection",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [SignalR(HubName = "HotDogHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            if (input == null || input.Count <= 0) return;

            log.LogInformation("Documents modified " + input.Count);
            log.LogInformation("First document Id " + input[0].Id);

            foreach (var item in input)
            {
                var imageUrl = item.GetPropertyValue<string>(nameof(ClientMessage.ImageUrl));
                var hotdogScore = item.GetPropertyValue<double>(nameof(ClientMessage.HotDog));
                var nonHotdogScore = item.GetPropertyValue<double>(nameof(ClientMessage.NonHotDog));

                var powerBiHotDogsApi = Environment.GetEnvironmentVariable("PowerBIHotDogsApi", EnvironmentVariableTarget.Process);

                var values = new Dictionary<string, dynamic>
                {
                    { "HotdogScore", hotdogScore },
                    { "NonHotdogScore", nonHotdogScore },
                    { "Id", item.Id },
                    { "Url", imageUrl },
                    { "PhotoDate", DateTime.UtcNow }
                };

                signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "clientMessage",
                    Arguments = new object[]
                   {
                        new ClientMessage
                        {
                            ImageUrl = imageUrl, 
                            HotDog = hotdogScore, 
                            NonHotDog = nonHotdogScore
                        }
                   }
                });

                log.LogInformation($"Timestamp: {DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}");

                var content = new StringContent($"[{JsonConvert.SerializeObject(values)}]");

                var response = HttpClient.PostAsync(powerBiHotDogsApi, content).GetAwaiter().GetResult();

                var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                log.LogInformation($"Power BI API Request Result: {string.Concat(response.StatusCode, responseString)}");
            }
        }
    }
}
