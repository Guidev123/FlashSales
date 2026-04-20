using FlashSales.Endpoints.Configurations;
using FlashSales.Infrastructure;
using Modules.Users.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerConfig();

builder.Services
    .AddInfrastructureModule(builder.Configuration)
    .AddUsersModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseSwaggerConfig();

app.Run();