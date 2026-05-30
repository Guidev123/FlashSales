using Dapper;
using FlashSales.Application.Abstractions;

namespace FlashSales.Infrastructure.Extensions
{
    public static class UnitOfWorkExtensions
    {
        public static CommandDefinition CreateCommand(
            this IUnitOfWork unitOfWork,
            string sql,
            object? param = null,
            CancellationToken cancellationToken = default)
            => new(sql, param, transaction: unitOfWork.Transaction, cancellationToken: cancellationToken);
    }
}