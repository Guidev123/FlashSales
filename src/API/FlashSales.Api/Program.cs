using FlashSales.Api.Configurations;

WebApplication
    .CreateBuilder(args)
    .AddConfiguration()
    .Build()
    .UseConfiguration()
    .Run();