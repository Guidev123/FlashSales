using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.SetDefaultRegistrationTypeRole;
using Modules.Users.Domain.Users.Enum;
using System.ComponentModel;

namespace Modules.Users.Endpoints.AccessManagement
{
    internal sealed class SetDefaultRegistrationTypeRoleEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/roles/{name}/default-registration-type", async (
                [Description("Name of the role to configure as default.")] string name,
                [Description("Registration type to configure. Allowed values: 'Social'.")] RegistrationType registrationType,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new SetDefaultRegistrationTypeRoleCommand(registrationType, name), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Roles.Configure)
              .WithSummary("Set the default role for a registration type")
              .WithDescription(
                  "Configures the given role to be automatically assigned to all new accounts of the specified registration type. " +
                  "For example, assigning 'customer' as the default role for the 'Social' registration type ensures every " +
                  "social-login user receives that role upon registration.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}
