using Npgsql;

namespace FlashSales.Infrastructure.Factories
{
    public sealed class SqlConnectionFactory(string connectionString)
    {
        public NpgsqlConnection Create() => new(connectionString);
    }
}