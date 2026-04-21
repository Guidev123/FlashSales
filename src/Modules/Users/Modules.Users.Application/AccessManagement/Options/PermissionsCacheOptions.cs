using System;
using System.Collections.Generic;
using System.Text;

namespace Modules.Users.Application.AccessManagement.Options
{
    public sealed class PermissionsCacheOptions
    {
        public const string SectionName = "PermissionsCache";

        public int CacheExpirationInSeconds { get; set; }
    }
}