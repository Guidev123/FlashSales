using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.Entities;
using Modules.Launches.Domain.Launches.Repositories;
using Modules.Launches.Domain.Sellers.Errors;
using Modules.Launches.Domain.Sellers.Repositories;

namespace Modules.Launches.Application.Launches.Features.Create
{
    internal sealed class CreateLaunchCommandHandler(
        ISellerRepository sellerRepository,
        ILaunchRepository launchRepository
        ) : ICommandHandler<CreateLaunchCommand, CreateLaunchResponse>
    {
        public async Task<Result<CreateLaunchResponse>> ExecuteAsync(CreateLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure<CreateLaunchResponse>(SellerErrors.NotFoundByUserId(request.UserId));
            }

            var launch = Launch.Create(
                seller.Id,
                request.ProductId,
                request.Title,
                request.Description
                );

            launchRepository.Add(launch);

            return new CreateLaunchResponse(launch.Id);
        }
    }
}