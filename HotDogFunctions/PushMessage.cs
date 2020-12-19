using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace HotDogFunctions
{
    public static class PushMessage
    {
        private static readonly HttpClient client = new HttpClient();

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
                var imageUrl = item.GetPropertyValue<string>("ImageUrl");
                var hotdogScore = item.GetPropertyValue<string>("Hot Dog");
                var nonHotdogScore = item.GetPropertyValue<string>("Non Hot Dog");

                signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "clientMessage",
                    Arguments = new object[] { new ClientMessage { ImageUrl = imageUrl, HotdogScore = hotdogScore, NonHotdogScore = nonHotdogScore } }
                });

                var powerBIHotDogsApi = Environment.GetEnvironmentVariable("PowerBIHotDogsApi", EnvironmentVariableTarget.Process);
                var values = new Dictionary<string, dynamic>
                {
                    { "HotdogScore", hotdogScore },
                    { "NonHotdogScore", nonHotdogScore },
                    { "Id", item.Id },
                    { "Url", imageUrl },
                    { "PhotoDate", DateTime.UtcNow }
                };

                log.LogInformation($"Timestamp: {DateTime.UtcNow.ToString()}");

                var content = new StringContent($"[{JsonConvert.SerializeObject(values)}]");

                var response = client.PostAsync(powerBIHotDogsApi, content).GetAwaiter().GetResult();

                var responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                log.LogInformation($"Power BI API Request Result: {String.Concat(response.StatusCode,responseString)}");
            }
        }
    }
}
