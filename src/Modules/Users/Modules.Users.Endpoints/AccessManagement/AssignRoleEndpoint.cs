using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.AssignRole;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class AssignRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/roles/{name}/users/{userId:guid}", async (
                string name,
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new AssignRoleCommand(userId, name), cancellationToken);

                return result.Match(() => Results.Created(), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Assign a role to an user")
              .RequireAuthorization(UsersPermissions.Roles.Assign);
        }
    }
}