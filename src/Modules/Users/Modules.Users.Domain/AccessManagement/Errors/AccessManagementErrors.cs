using FlashSales.Domain.Results;
using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Domain.AccessManagement.Errors
{
    public static class AccessManagementErrors
    {
        public static Error RoleNotFound(string roleName) => Error.NotFound(
            "AcessManagement.RoleNotFound",
            $"The role '{roleName}' was not found"
            );

        public static Error PermissionNotFound(string permission) => Error.NotFound(
            "AcessManagement.PermissionNotFound",
            $"The permission '{permission}' was not found"
            );

        public static readonly Error InvalidRoleName = Error.Problem(
          "AccessManagement.InvalidRoleName",
          "Role name must not be empty");

        public static readonly Error InvalidPermissionCode = Error.Problem(
            "AccessManagement.InvalidPermissionCode",
            "Permission code must follow the format 'resource:action:scope'");

        public static readonly Error InvalidUserId = Error.Invalid(
            "AccessManagement.InvalidUserId",
            "User identifier must not be empty");

        public static readonly Error InvalidRoleForRegistrationType = Error.Invalid(
            "AccessManagement.InvalidRoleForRegistrationType",
            "Invalid role for registration type");

        public static Error PermissionsNotFoundForUser(Guid userId) => Error.NotFound(
            "AccessManagement.PermissionsNotFoundForUser",
            $"No permissions found for user with ID '{userId}'");

        public static Error PermissionsNotFoundForUser(string identityProviderId) => Error.NotFound(
            "AccessManagement.PermissionsNotFoundForUser",
            $"No permissions found for user with Identity Provider ID '{identityProviderId}'");
    }
}