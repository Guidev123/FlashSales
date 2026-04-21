namespace Modules.Users.Domain.AccessManagement.Models
{
    public sealed class Permission
    {
        public Permission(string code)
        {
            Code = code;
        }

        public string Code { get; private set; } = null!;
    }
}