using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HotDogFunctions
{
    public static class PushMessage
    {
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
                var scores = item.GetPropertyValue<string>("Scores");

                signalRMessages.AddAsync(new SignalRMessage
                {
                    Target = "clientMessage",
                    Arguments = new object[] { new ClientMessage { ImageUrl = imageUrl, Scores = scores } }
                });
            }
        }
    }
}
