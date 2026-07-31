using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Features.GetById;
using System.Security.Claims;

namespace Modules.Orders.Endpoints.Orders
{
    internal sealed class GetOrderByIdEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/orders/{orderId:guid}", async (
                Guid orderId,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(
                    new GetOrderByIdQuery(orderId, claimsPrincipal.GetUserId()),
                    cancellationToken);

                return result.Match(Results.Ok, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(OrdersPermissions.Orders.ReadOwn)
              .WithSummary("Get an order by id")
              .WithDescription("Returns the details of an order owned by the authenticated customer.")
              .Produces<OrderResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}
