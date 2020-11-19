using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace JustLearnIT.AzureServices
{
    public static class BlobStorageService
    {
        public static string BlobConnectionString { get; set; }

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
    }
}

