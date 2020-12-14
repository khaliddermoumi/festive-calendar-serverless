using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotDogFunctions
{
    public static class PushMessage
    {
        [FunctionName("PushMessage")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "hotdogsphotos",
            collectionName: "photosclassification",
            ConnectionStringSetting = "CosmosDBConnection", 
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [SignalR(HubName = "HotDogHub")] IAsyncCollector<SignalRMessage> signalRMessages,
            [EventHub("$Default", Connection = "EventHubConnection")]IAsyncCollector<string> outputEvents,
            ILogger log)
        {
            if (input == null || input.Count <= 0) return;

            log.LogInformation("Documents modified " + input.Count);
            log.LogInformation("First document Id " + input[0].Id);

            foreach (var item in input)
            {
                var imageUrl = item.GetPropertyValue<string>("ImageUrl");
                var scores = item.GetPropertyValue<string>("Scores");

                await signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "clientMessage",
                    Arguments = new object[] { new ClientMessage { ImageUrl = imageUrl, Scores = scores } }
                });

                var hubMessage = new {
                    ImageUrl = imageUrl,
                    Scores = scores
                };

                // Send message to Event Hub, to propagate message to the Power BI Live Streaming Dataset
                await outputEvents.AddAsync(JsonConvert.SerializeObject(hubMessage));
            }
        }
    }
}
