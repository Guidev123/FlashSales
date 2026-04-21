using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.UseCases.SetDefaultRegistrationTypeRole;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class SetDefaultRegistrationTypeRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles/{name}/default-registration-type", async (
                string name,
                RegistrationType registrationType,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new SetDefaultRegistrationTypeRoleCommand(registrationType, name), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .WithDescription("Assigns default roles to a specific account type")
              .RequireAuthorization(UsersPermissions.Roles.Configure);
        }
    }
}