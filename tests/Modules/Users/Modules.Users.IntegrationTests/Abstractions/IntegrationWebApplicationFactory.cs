using FlashSales.Application.Bus;
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
using Testcontainers.Keycloak;
using Testcontainers.PostgreSql;

namespace Modules.Users.IntegrationTests.Abstractions
{
    public class IntegrationWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private const string KeycloakAdminUser = "admin";
        private const string KeycloakAdminPassword = "admin";
        private const string ConfidentialClientId = "flash-sales-test-client";
        private const string RealmName = "flash-sales-dev";
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            string keycloakAddress = _keycloakContainer.GetBaseAddress();
            string realmUrl = $"{keycloakAddress}realms/{RealmName}";

            builder.UseSetting("ConnectionStrings:Postgres", _postgresContainer.GetConnectionString());
            builder.UseSetting("ServiceBus:ConnectionString",
                "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dGVzdA==");
            builder.UseSetting("BlobStorage:ConnectionString",
                "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=dGVzdA==;EndpointSuffix=core.windows.net");
            builder.UseSetting("Authentication:MetadataAddress", $"{realmUrl}/.well-known/openid-configuration");
            builder.UseSetting("Authentication:TokenValidationParameters:ValidIssuer", realmUrl);

            builder.ConfigureAppConfiguration(cfg =>
                cfg.AddJsonFile(
                    Path.Combine(AppContext.BaseDirectory, "modules.users.Testing.json"),
                    optional: true));

            builder.ConfigureServices(services =>
            {
                RemoveHostedServices(services);
                ReplaceEventBus(services);
                ReplaceKeycloakOptions(services, keycloakAddress);
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
            await Task.WhenAll(
                _postgresContainer.StartAsync(),
                _keycloakContainer.StartAsync());

            _confidentialClientSecret = await KeycloakAdminHelper.SetupTestClientAsync(
                _keycloakContainer.GetBaseAddress(),
                KeycloakAdminUser,
                KeycloakAdminPassword,
                ConfidentialClientId);

            await MigrateAndSeedAsync();
        }

        public new async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _keycloakContainer.DisposeAsync();
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

        private static void ReplaceEventBus(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEventBus));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IEventBus, NoOpEventBus>();
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

        private async Task MigrateAndSeedAsync()
        {
            using var scope = Services.CreateScope();
            var sp = scope.ServiceProvider;

            await sp.GetRequiredService<UsersDbContext>().Database.MigrateAsync();
            await sp.GetRequiredService<CatalogDbContext>().Database.MigrateAsync();

            await SeedRolesAsync();
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