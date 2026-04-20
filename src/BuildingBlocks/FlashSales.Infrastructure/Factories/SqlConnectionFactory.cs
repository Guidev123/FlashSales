using Microsoft.Data.SqlClient;

namespace FlashSales.Infrastructure.Factories
{
    public sealed class SqlConnectionFactory(string connectionString)
    {
        public SqlConnection Create() => new(connectionString);
    }
}