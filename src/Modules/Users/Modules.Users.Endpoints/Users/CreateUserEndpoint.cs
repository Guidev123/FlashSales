using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.Create;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class CreateUserEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/users", async (
                CreateUserCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(command, cancellationToken);

                return result.Match(() => Results.Created($"users/{result.Value.Id}", result.Value), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithSummary("Register a new user")
              .WithDescription(
                  "Creates a user account in the system and provisions the corresponding identity in the identity provider. " +
                  "Returns the new user's internal ID, identity-provider ID, and email address.")
              .Produces<CreateUserResponse>(StatusCodes.Status201Created)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status409Conflict);
        }
    }
}
