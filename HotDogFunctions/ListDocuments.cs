using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HotDogFunctions
{
    public static class ListDocuments
    {
        [FunctionName("ListDocuments")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "list")]
            HttpRequest req,
            [CosmosDB(
                "hotdogsphotos",
                "photosclassification",
                ConnectionStringSetting = "CosmosDBConnection",
                SqlQuery = "SELECT * FROM c order by c._ts desc")]
            IEnumerable<ClientMessage> predictions,
            ILogger log)
        {
            return new OkObjectResult(predictions);
        }
    }
}