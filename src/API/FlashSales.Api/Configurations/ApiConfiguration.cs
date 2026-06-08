using FlashSales.Api.Extensions;
using FlashSales.Api.Middlewares;
using FlashSales.Endpoints.Configurations;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Catalog.Infrastructure;
using Modules.Launches.Infrastructure;
using Modules.Users.Infrastructure;
using Serilog;

namespace FlashSales.Api.Configurations;

public static class ApiConfiguration
{
    private static readonly string[] Modules = ["users", "catalog", "launches"];

    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder
            .AddModuleConfiguration()
            .AddLogging()
            .AddOpenApi()
            .AddCorsPolicy()
            .AddMiddlewares()
            .AddModules();

        return builder;
    }

    public static WebApplication UseConfiguration(this WebApplication app)
    {
        app.UseOpenApi()
            .UseLogging()
            .UseExceptionHandler()
            .UseHttpsRedirection()
            .UseCors("AllowWebApp")
            .UseAuthentication()
            .UseAuthorization()
            .UseMiddleware<AccountActivationMiddleware>();

        app.MapEndpoints();

        return app;
    }

    private static WebApplicationBuilder AddModuleConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddModuleConfiguration(Modules, builder.Environment);
        return builder;
    }

    private static WebApplicationBuilder AddLogging(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsEnvironment("Testing"))
            builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

        return builder;
    }

    private static WebApplicationBuilder AddOpenApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApiConfig(builder.Configuration);
        return builder;
    }

    private static WebApplicationBuilder AddCorsPolicy(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowWebApp", policy =>
            {
                var raw = builder.Configuration["WebAppEndpoints"];
                if (raw is null) return;

                policy
                    .WithOrigins([.. raw.Split(',')])
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return builder;
    }

    private static WebApplicationBuilder AddMiddlewares(this WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddTransient<AccountActivationMiddleware>();
        return builder;
    }

    private static WebApplicationBuilder AddModules(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddInfrastructureModule(builder.Configuration,
            [
                ..UsersModule.Assemblies,
                ..CatalogModule.Assemblies
            ])
            .AddUsersModule(builder.Configuration)
            .AddCatalogModule(builder.Configuration)
            .AddLaunchesModule(builder.Configuration);

        return builder;
    }

    private static WebApplication UseOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseOpenApiConfig(app.Configuration);
        }

        return app;
    }

    private static WebApplication UseLogging(this WebApplication app)
    {
        if (!app.Environment.IsEnvironment("Testing"))
            app.UseSerilogRequestLogging();

        return app;
    }
}