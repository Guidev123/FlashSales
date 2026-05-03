using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace FlashSales.Endpoints.Configurations
{
    public static class DocumentationConfiguration
    {
        public static void AddOpenApiConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    document.Components.SecuritySchemes["oauth2"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(configuration["Keycloak:AuthorizationUrl"]!),
                                TokenUrl = new Uri(configuration["Keycloak:TokenUrl"]!),
                                Scopes = new Dictionary<string, string>
                                {
                                    { "openid", "OpenID Connect" },
                                    { "profile", "Profile information" }
                                }
                            }
                        }
                    };

                    document.Security ??= [];
                    document.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2", document)] = ["openid", "profile"]
                    });

                    return Task.CompletedTask;
                });
            });
        }

        public static void UseOpenApiConfig(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseStaticFiles();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi/v1.json", "FlashSales API v1");
                c.DisplayRequestDuration();
                c.OAuthClientId(configuration["Keycloak:SwaggerClientId"]);
                c.OAuthAppName("FlashSales Swagger");
                c.OAuthScopeSeparator(" ");
                c.OAuthUsePkce();
            });
        }
    }
}