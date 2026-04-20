using Modules.Users.Domain.Users.Enum;

namespace Modules.Users.Domain.Users.Models
{
    public sealed class RegistrationTypeRoles
    {
        public RegistrationTypeRoles(RegistrationType type, string roleName)
        {
            Type = type;
            RoleName = roleName;
        }

        public RegistrationType Type { get; private set; }
        public string RoleName { get; private set; }
    }
}