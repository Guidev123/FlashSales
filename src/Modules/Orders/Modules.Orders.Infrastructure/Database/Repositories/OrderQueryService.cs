using Dapper;
using Modules.Orders.Application.Abstractions;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Services;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class OrderQueryService(IOrdersUnitOfWork unitOfWork) : IOrderQueryService
    {
        public async Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    o."Id",
                    o."LaunchId",
                    ls."Title" AS "LaunchTitle",
                    o."ProductId",
                    o."SellerId",
                    o."Quantity",
                    o."UnitPrice",
                    o."TotalAmount",
                    o."OrderCode",
                    o."Status",
                    o."ExpiresAt",
                    o."ConfirmedAt",
                    o."Reason",
                    o."CreatedOn"
                FROM orders."Orders" o
                LEFT JOIN orders."Launches" ls ON ls."Id" = o."LaunchId"
                WHERE o."Id" = @OrderId AND o."CustomerId" = @CustomerId
                """;

            var row = await unitOfWork.Connection.QuerySingleOrDefaultAsync(sql, new { OrderId = orderId, CustomerId = customerId });

            return row is null ? null : MapToResponse(row);
        }

        public async Task<IReadOnlyCollection<OrderResponse>> GetByCustomerAsync(
            Guid customerId,
            int page,
            int size,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    o."Id",
                    o."LaunchId",
                    ls."Title" AS "LaunchTitle",
                    o."ProductId",
                    o."SellerId",
                    o."Quantity",
                    o."UnitPrice",
                    o."TotalAmount",
                    o."OrderCode",
                    o."Status",
                    o."ExpiresAt",
                    o."ConfirmedAt",
                    o."Reason",
                    o."CreatedOn"
                FROM orders."Orders" o
                LEFT JOIN orders."Launches" ls ON ls."Id" = o."LaunchId"
                WHERE o."CustomerId" = @CustomerId
                ORDER BY o."CreatedOn" DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY
                """;

            var rows = await unitOfWork.Connection.QueryAsync(sql, new
            {
                CustomerId = customerId,
                Offset = (page - 1) * size,
                Size = size
            }).WaitAsync(cancellationToken);

            return rows.Select(MapToResponse).ToList().AsReadOnly();
        }

        public Task<int> GetByCustomerTotalCountAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM orders."Orders" o
                WHERE o."CustomerId" = @CustomerId
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { CustomerId = customerId });
        }

        public Task<bool> HasActiveOrderAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM orders."Orders" o
                    WHERE o."CustomerId" = @CustomerId
                      AND o."LaunchId" = @LaunchId
                      AND o."Status" IN ('AwaitingPayment', 'PaymentProcessing')
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(sql, new { CustomerId = customerId, LaunchId = launchId });
        }

        public Task<int> GetConfirmedQuantityAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COALESCE(SUM(o."Quantity"), 0)
                FROM orders."Orders" o
                WHERE o."CustomerId" = @CustomerId
                  AND o."LaunchId" = @LaunchId
                  AND o."Status" = 'Confirmed'
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { CustomerId = customerId, LaunchId = launchId });
        }

        private static OrderResponse MapToResponse(dynamic row)
        {
            return new OrderResponse(
                Id: row.Id,
                LaunchId: row.LaunchId,
                LaunchTitle: row.LaunchTitle,
                ProductId: row.ProductId,
                SellerId: row.SellerId,
                Quantity: row.Quantity,
                UnitPrice: row.UnitPrice,
                TotalAmount: row.TotalAmount,
                OrderCode: row.OrderCode,
                Status: (string)row.Status,
                ExpiresAt: row.ExpiresAt,
                ConfirmedAt: row.ConfirmedAt,
                Reason: row.Reason,
                CreatedOn: row.CreatedOn
                );
        }
    }
}