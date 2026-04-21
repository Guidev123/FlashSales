using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.Create;

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
              .WithDescription("Create an user");
        }
    }
}