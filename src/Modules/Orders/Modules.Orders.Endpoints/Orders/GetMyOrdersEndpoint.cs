using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Features.GetMine;
using System.Security.Claims;

namespace Modules.Orders.Endpoints.Orders
{
    internal sealed class GetMyOrdersEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/orders/me", async (
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken,
                int page = 1,
                int size = 20
                ) =>
            {
                var result = await sender.SendAsync(
                    new GetMyOrdersQuery(claimsPrincipal.GetUserId(), page, size),
                    cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(OrdersPermissions.Orders.ReadOwn)
              .WithSummary("List the authenticated customer's orders")
              .Produces<PagedResult<OrderResponse>>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized);
        }
    }
}
