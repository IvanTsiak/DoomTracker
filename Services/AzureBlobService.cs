using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DoomTracker.Services;

public class AzureBlobService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "uploads";

    public AzureBlobService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return null!;
        }
        
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var blobClient = containerClient.GetBlobClient(uniqueFileName);
        
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }
        
        return blobClient.Uri.ToString();
    }
}