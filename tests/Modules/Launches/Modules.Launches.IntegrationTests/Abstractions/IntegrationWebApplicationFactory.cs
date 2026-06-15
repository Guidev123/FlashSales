using Azure.Messaging.ServiceBus;
using DotNet.Testcontainers.Builders;
using FlashSales.Infrastructure.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Launches.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.ServiceBus;

namespace Modules.Launches.IntegrationTests.Abstractions
{
    public class IntegrationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("flashsales_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly ServiceBusContainer _serviceBusContainer = new ServiceBusBuilder()
            .WithAcceptLicenseAgreement(true)
            .WithResourceMapping(
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Abstractions", "servicebus.config.json")),
                new FileInfo("/ServiceBus_Emulator/ConfigFiles/Config.json"))
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
            builder.UseSetting("Authentication:MetadataAddress",
                "https://test.auth/.well-known/openid-configuration");
            builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuer",
                "https://test.auth/realms/flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:AdminUrl", "https://test.keycloak/admin/realms/");
            builder.UseSetting("Users:KeyCloak:BaseUrl", "https://test.keycloak/realms/");
            builder.UseSetting("Users:KeyCloak:CurrentRealm", "flash-sales-dev");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientId", "test-client");
            builder.UseSetting("Users:KeyCloak:ConfidentialClientSecret", "test-secret");

            builder.ConfigureAppConfiguration(cfg =>
                cfg.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "modules.launches.Testing.json"),
                    optional: true));

            builder.ConfigureServices(services =>
            {
                RemoveHostedServices(services);
                ReplaceServiceBusClient(services);
                ReplaceSqlConnectionFactory(services);
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            return base.CreateHost(builder);
        }

        public async Task InitializeAsync()
        {
            await _serviceBusContainer.StartAsync();
            await _postgresContainer.StartAsync();
            await MigrateAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _serviceBusContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("""
                DELETE FROM launches."StockReservations";
                DELETE FROM launches."Launches";
                DELETE FROM launches."Sellers";
                DELETE FROM launches."OutboxMessageConsumers";
                DELETE FROM launches."OutboxMessages";
                DELETE FROM launches."InboxMessageConsumers";
                DELETE FROM launches."InboxMessages";
                """, connection);

            await cmd.ExecuteNonQueryAsync();
        }

        private static void RemoveHostedServices(IServiceCollection services)
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);
        }

        private void ReplaceServiceBusClient(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ServiceBusClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new ServiceBusClient(_serviceBusContainer.GetConnectionString()));
        }

        private void ReplaceSqlConnectionFactory(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SqlConnectionFactory));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new SqlConnectionFactory(_postgresContainer.GetConnectionString()));
        }

        private async Task MigrateAsync()
        {
            using var scope = Services.CreateScope();
            await scope.ServiceProvider.GetRequiredService<LaunchesDbContext>().Database.MigrateAsync();
        }
    }
}
