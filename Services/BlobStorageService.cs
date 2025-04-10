using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Azure;

namespace FutureTechAcademy.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _container;

        public BlobStorageService(IConfiguration config)
        {
            var blobServiceClient = new BlobServiceClient(config["AzureBlobStorage:ConnectionString"]);
            _container = blobServiceClient.GetBlobContainerClient(config["AzureBlobStorage:ContainerName"]);
            _container.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile file, string studentId)
        {
            string blobName = $"student-{studentId}{Path.GetExtension(file.FileName)}";
            var blobClient = _container.GetBlobClient(blobName);

            // Delete if exists to ensure clean overwrite
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            // Upload with explicit overwrite setting
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobName; // Return just the blob name
        }

        public async Task<Stream> GetImageStreamAsync(string blobName)
        {
            try
            {
                var blobClient = _container.GetBlobClient(blobName);
                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Or throw specific exception
            }
        }
    }

}
