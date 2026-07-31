using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.Create;
using System.Security.Claims;

namespace Modules.Orders.Endpoints.Orders
{
    internal sealed class CreateOrderEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/orders", async (
                CreateOrderRequest request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken
                ) =>
            {
                var command = new CreateOrderCommand(
                    claimsPrincipal.GetUserId(),
                    claimsPrincipal.GetEmail(),
                    request.LaunchId,
                    request.Quantity
                    );

                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(success => Results.Ok(success), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(OrdersPermissions.Orders.Create)
              .WithSummary("Create an order for a Launch")
              .WithDescription(
                  "Reserves stock for the given Launch and starts a checkout session with the payment gateway. " +
                  "Returns the URL the customer should be redirected to in order to complete the payment.")
              .Produces<CreateOrderResponse>(StatusCodes.Status200OK)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }

        sealed record CreateOrderRequest(Guid LaunchId, int Quantity);
    }
}
