using FlashSales.Api.Extensions;
using FlashSales.Api.Middlewares;
using FlashSales.Endpoints.Configurations;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Infrastructure;
using Modules.Catalog.Infrastructure;
using Modules.Users.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfig();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddTransient<AccountActivationMiddleware>();
builder.Configuration.AddModuleConfiguration([
    "users",
    "catalog"
    ]);

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Host.UseSerilog((context, loggerConfig)
        => loggerConfig.ReadFrom.Configuration(context.Configuration));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebApp", policy =>
    {
        if (builder.Configuration["WebAppEndpoints"] is null) return;

        var origins = builder.Configuration["WebAppEndpoints"]!.Split(',');

        policy
            .WithOrigins([.. origins])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services
    .AddInfrastructureModule(builder.Configuration)
    .AddUsersModule(builder.Configuration)
    .AddCatalogModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerConfig();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseSerilogRequestLogging();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("AllowWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AccountActivationMiddleware>();

app.MapEndpoints();

app.Run();