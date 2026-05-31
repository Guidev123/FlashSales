using FlashSales.Application.Storage;
using FlashSales.Domain.Results;

namespace Modules.Users.IntegrationTests.Abstractions
{
    internal sealed class FakeBlobStorageService : IBlobStorageService
    {
        internal const string FakeUrl = "https://fake.blob.core.windows.net/images/profile.jpg";

        public Task<Result<string>> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(FakeUrl));

        public Task<Result> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }
}
