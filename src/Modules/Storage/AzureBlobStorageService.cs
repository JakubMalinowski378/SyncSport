using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Storage;

internal sealed class AzureBlobStorageService(IOptions<AzureBlobStorageOptions> options) : IImageStorageService
{
    private readonly BlobContainerClient _containerClient = new(options.Value.ConnectionString, options.Value.ContainerName);

    public async Task<string> AddAsync(Stream content, string contentType, string fileName, CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var blobClient = _containerClient.GetBlobClient(fileName);
        var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

        await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<IEnumerable<string>> AddRangeAsync(IEnumerable<(Stream content, string contentType, string fileName)> files, CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        var urls = new List<string>();
        foreach (var (content, contentType, fileName) in files)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, cancellationToken);
            urls.Add(blobClient.Uri.ToString());
        }

        return urls;
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
        {
            var blobName = new BlobUriBuilder(uri).BlobName;

            if (string.IsNullOrWhiteSpace(blobName))
            {
                return;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
    }
}