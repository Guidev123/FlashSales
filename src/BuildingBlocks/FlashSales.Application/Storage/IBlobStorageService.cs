using FlashSales.Domain.Results;

namespace FlashSales.Application.Storage
{
    public interface IBlobStorageService
    {
        Task<Result> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);

        Task<Result<string>> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default);
    }
}