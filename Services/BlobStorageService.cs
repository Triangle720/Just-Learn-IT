using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace JustLearnIT.Services
{
    public static class BlobStorageService
    {
        private static string BlobConnectionString { get; set; }

        public static void Create(string secret)
        {
            BlobConnectionString = secret;
        }

        public async static Task<CloudBlobContainer> GetBlobContainer(string containerReference)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(BlobConnectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerReference);

            if (await cloudBlobContainer.CreateIfNotExistsAsync())
            {
                await cloudBlobContainer.SetPermissionsAsync(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                    );
            }

            return cloudBlobContainer;
        }

        public static async Task<string> GetTextFromFileByName(string itemName, string containerReference)
        {
            var cloudBlobContainer = await GetBlobContainer(containerReference);
            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(itemName);
            return await cloudBlockBlob.DownloadTextAsync();
        }

        public static async Task<FileContentResult> GetVideo(string itemName, string containerReference)
        {
            var cloudBlobContainer = await GetBlobContainer(containerReference);
            var cloudBlob = cloudBlobContainer.GetBlobReference(itemName);
            //var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(itemName);
            var stream = new MemoryStream();
            await cloudBlob.DownloadToStreamAsync(stream);
            var result = new FileContentResult(stream.ToArray(), "application/octet-stream")
            {
                EnableRangeProcessing = true
            };
            return result;
        }
    }
}

