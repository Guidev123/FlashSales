using FlashSales.Application.Messaging;
using Modules.Launches.Application.Launches.Dtos;

namespace Modules.Launches.Application.Launches.Features.GetById
{
    public sealed record GetLaunchByIdQuery(Guid LaunchId) : IQuery<LaunchResponse>;
}
