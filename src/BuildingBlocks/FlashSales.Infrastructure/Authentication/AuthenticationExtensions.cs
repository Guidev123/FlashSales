using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace FlashSales.Infrastructure.Authentication
{
    internal static class AuthenticationExtensions
    {
        internal static IServiceCollection AddAuthenticationInternal(this IServiceCollection services)
        {
            services.AddAuthentication().AddJwtBearer();
            services.AddAuthorization();

            services.AddHttpContextAccessor();

            services.ConfigureOptions<JwtBearerConfigureOptions>();

            return services;
        }
    }
}