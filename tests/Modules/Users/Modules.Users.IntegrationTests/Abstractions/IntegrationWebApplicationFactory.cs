using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using FlashSales.Infrastructure.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Modules.Catalog.Infrastructure.Database;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Identity;
using Modules.Users.IntegrationTests.Abstractions.Files;
using Npgsql;
using DotNet.Testcontainers.Builders;
using Testcontainers.Azurite;
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;
using Testcontainers.ServiceBus;

namespace Modules.Users.IntegrationTests.Abstractions
{
    public class IntegrationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string KeycloakAdminUser = "admin";
        private const string KeycloakAdminPassword = "admin";
        private const string ConfidentialClientId = "flash-sales-test-client";
        private const string RealmName = "flash-sales-dev";
        private const string BlobContainerName = "users-test";

        private string _confidentialClientSecret = string.Empty;

        private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("flashsales_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
            .WithImage("quay.io/keycloak/keycloak:26.6.1")
            .WithUsername(KeycloakAdminUser)
            .WithPassword(KeycloakAdminPassword)
            .WithResourceMapping(
                new FileInfo(Paths.KeycloakDataImportRealmLocal),
                new FileInfo(Paths.KeycloakDataImportRealm))
            .WithCommand("--import-realm")
            .Build();

        private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(10000))
            .Build();

        private readonly ServiceBusContainer _serviceBusContainer = new ServiceBusBuilder()
            .WithAcceptLicenseAgreement(true)
            .WithResourceMapping(
                new FileInfo(Path.Combine(AppContext.BaseDirectory, "Abstractions", "servicebus.config.json")),
                new FileInfo("/ServiceBus_Emulator/ConfigFiles/Config.json"))
            .Build();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            string keycloakAddress = _keycloakContainer.GetBaseAddress();
            string realmUrl = $"{keycloakAddress}realms/{RealmName}";

            builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
            builder.UseSetting("BlobStorage:ConnectionString", _azuriteContainer.GetConnectionString());
            builder.UseSetting("Authentication:MetadataAddress", $"{realmUrl}/.well-known/openid-configuration");
            builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuer", realmUrl);

            builder.ConfigureAppConfiguration(cfg =>
                cfg.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "modules.users.Testing.json"),
                    optional: true));

            builder.ConfigureServices(services =>
            {
                RemoveHostedServices(services);
                ReplaceServiceBusClient(services);
                ReplaceKeycloakOptions(services, keycloakAddress);
                ReplaceSqlConnectionFactory(services);
                ReplaceBlobServiceClient(services);
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

            await Task.WhenAll(
                _postgresContainer.StartAsync(),
                _keycloakContainer.StartAsync(),
                _azuriteContainer.StartAsync());

            _confidentialClientSecret = await KeycloakAdminHelper.SetupTestClientAsync(
                _keycloakContainer.GetBaseAddress(),
                KeycloakAdminUser,
                KeycloakAdminPassword,
                ConfidentialClientId);

            await Task.WhenAll(
                MigrateAndSeedAsync(),
                CreateBlobContainerAsync());
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _keycloakContainer.DisposeAsync();
            await _azuriteContainer.DisposeAsync();
            await _serviceBusContainer.DisposeAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand("""
                DELETE FROM users."UserRoles";
                DELETE FROM users."Users";
                DELETE FROM users."OutboxMessageConsumers";
                DELETE FROM users."OutboxMessages";
                DELETE FROM users."InboxMessageConsumers";
                DELETE FROM users."InboxMessages";
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

        private void ReplaceKeycloakOptions(IServiceCollection services, string keycloakAddress)
        {
            var options = new KeyCloakOptions
            {
                AdminUrl = $"{keycloakAddress}admin/realms/",
                CurrentRealm = RealmName,
                BaseUrl = $"{keycloakAddress}realms/",
                ConfidentialClientId = ConfidentialClientId,
                ConfidentialClientSecret = _confidentialClientSecret
            };

            services.RemoveAll<IOptions<KeyCloakOptions>>();
            services.AddSingleton<IOptions<KeyCloakOptions>>(new OptionsWrapper<KeyCloakOptions>(options));
        }

        private void ReplaceSqlConnectionFactory(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(SqlConnectionFactory));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new SqlConnectionFactory(_postgresContainer.GetConnectionString()));
        }

        private void ReplaceBlobServiceClient(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(BlobServiceClient));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton(new BlobServiceClient(_azuriteContainer.GetConnectionString()));
        }

        private async Task MigrateAndSeedAsync()
        {
            using var scope = Services.CreateScope();
            var sp = scope.ServiceProvider;

            await sp.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();

            await SeedRolesAsync();
        }

        private async Task CreateBlobContainerAsync()
        {
            var serviceClient = new BlobServiceClient(_azuriteContainer.GetConnectionString());
            var containerClient = serviceClient.GetBlobContainerClient(BlobContainerName);
            await containerClient.CreateIfNotExistsAsync();
        }

        private async Task SeedRolesAsync()
        {
            string script = await File.ReadAllTextAsync(Paths.RolesSeedDataSql);

            await using var connection = new NpgsqlConnection(_postgresContainer.GetConnectionString());
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(script, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
