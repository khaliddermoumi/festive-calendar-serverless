using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HotDogFunctions
{
    public static class UploadImage
    {
        [FunctionName("UploadImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")] HttpRequestMessage req,
            ILogger log,
            [Blob("sample-images", FileAccess.Write, Connection = "AzureStorageConnectionString")] CloudBlobContainer cloudBlobContainer)
        {
            await cloudBlobContainer.CreateIfNotExistsAsync();

            var multipartMemoryStreamProvider = new MultipartMemoryStreamProvider();

            await req.Content.ReadAsMultipartAsync(multipartMemoryStreamProvider);

            var file = multipartMemoryStreamProvider.Contents[0];

            var fileInfo = file.Headers.ContentDisposition;
            
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileInfo.FileName)}";

            blobName = blobName.Replace("\"", "");

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

            await using (var fileStream = await file.ReadAsStreamAsync())
            {
                await cloudBlockBlob.UploadFromStreamAsync(fileStream);
            }

            return new OkObjectResult(new { name = blobName });
        }
    }
}
