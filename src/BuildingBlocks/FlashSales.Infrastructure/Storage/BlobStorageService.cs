using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FlashSales.Application.Storage;
using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Storage
{
    internal sealed class BlobStorageService(
        BlobServiceClient client,
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger
        ) : IBlobStorageService
    {
        private readonly BlobStorageOptions _storageOptions = options.Value;

        private static readonly Error FailedToAccessBlob = Error.Problem(
            "BlobStorage.FailedToAccessBlob",
            "Something has failed to access blob storage");

        public async Task<Result> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = client.GetBlobContainerClient(_storageOptions.ContainerName);

                var blob = container.GetBlobClient(fileId.ToString());

                await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something has failed to access blob storage");

                return Result.Failure(FailedToAccessBlob);
            }
        }

        public async Task<Result<string>> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var container = client.GetBlobContainerClient(_storageOptions.ContainerName);

                var fileId = Guid.NewGuid();

                var blob = container.GetBlobClient(fileId.ToString());

                await blob.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = contentType
                }, cancellationToken: cancellationToken);

                return blob.Uri.ToString();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Something has failed to access blob storage");

                return Result.Failure<string>(FailedToAccessBlob);
            }
        }
    }
}