using FlashSales.Domain.Results;

namespace FlashSales.Application.Authorization
{
    public interface IPermissionService
    {
        Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default);
    }
}