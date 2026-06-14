using Dapper;
using Modules.Launches.Application.Abstractions;
using Modules.Launches.Application.Launches.Dtos;
using Modules.Launches.Application.Launches.Services;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class LaunchQueryService(ILaunchesUnitOfWork unitOfWork) : ILaunchQueryService
    {
        public async Task<LaunchResponse?> GetByIdAsync(Guid launchId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    l."Id",
                    l."SellerId",
                    l."ProductId",
                    l."Title",
                    l."Description",
                    l."DiscountedPrice",
                    l."OriginalPrice",
                    l."TotalQuantity",
                    l."ReservedQuantity",
                    l."StartAt",
                    l."EndAt",
                    l."Status",
                    l."CreatedOn"
                FROM launches."Launches" l
                WHERE l."Id" = @LaunchId
                """;

            var row = await unitOfWork.Connection.QuerySingleOrDefaultAsync(sql, new { LaunchId = launchId });

            if (row is null) return null;

            return MapToResponse(row);
        }

        public async Task<IReadOnlyCollection<LaunchResponse>> GetAllAsync(
            int page,
            int size,
            string? status,
            Guid? sellerId,
            Guid? productId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    l."Id",
                    l."SellerId",
                    l."ProductId",
                    l."Title",
                    l."Description",
                    l."DiscountedPrice",
                    l."OriginalPrice",
                    l."TotalQuantity",
                    l."ReservedQuantity",
                    l."StartAt",
                    l."EndAt",
                    l."Status",
                    l."CreatedOn"
                FROM launches."Launches" l
                WHERE (@Status IS NULL OR l."Status" = @Status)
                  AND (@SellerId IS NULL OR l."SellerId" = @SellerId)
                  AND (@ProductId IS NULL OR l."ProductId" = @ProductId)
                ORDER BY l."CreatedOn" DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY
                """;

            var rows = await unitOfWork.Connection.QueryAsync(sql, new
            {
                Status = status,
                SellerId = sellerId,
                ProductId = productId,
                Offset = (page - 1) * size,
                Size = size
            }).WaitAsync(cancellationToken);

            return rows.Select(MapToResponse).ToList().AsReadOnly();
        }

        public async Task<int> GetTotalCountAsync(
            string? status,
            Guid? sellerId,
            Guid? productId,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM launches."Launches" l
                WHERE (@Status IS NULL OR l."Status" = @Status)
                  AND (@SellerId IS NULL OR l."SellerId" = @SellerId)
                  AND (@ProductId IS NULL OR l."ProductId" = @ProductId)
                """;

            var count = await unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new
            {
                Status = status,
                SellerId = sellerId,
                ProductId = productId
            });

            return count;
        }

        public async Task<IReadOnlyCollection<LaunchResponse>> GetBySellerAsync(
            Guid sellerId,
            int page,
            int size,
            CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    l."Id",
                    l."SellerId",
                    l."ProductId",
                    l."Title",
                    l."Description",
                    l."DiscountedPrice",
                    l."OriginalPrice",
                    l."TotalQuantity",
                    l."ReservedQuantity",
                    l."StartAt",
                    l."EndAt",
                    l."Status",
                    l."CreatedOn"
                FROM launches."Launches" l
                WHERE l."SellerId" = @SellerId
                ORDER BY l."CreatedOn" DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY
                """;

            var rows = await unitOfWork.Connection.QueryAsync(sql, new
            {
                SellerId = sellerId,
                Offset = (page - 1) * size,
                Size = size
            }).WaitAsync(cancellationToken);

            return rows.Select(MapToResponse).ToList().AsReadOnly();
        }

        public async Task<int> GetBySellerTotalCountAsync(Guid sellerId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM launches."Launches" l
                WHERE l."SellerId" = @SellerId
                """;

            return await unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new { SellerId = sellerId });
        }

        private static LaunchResponse MapToResponse(dynamic row)
        {
            decimal? discountedPrice = row.DiscountedPrice;
            decimal? originalPrice = row.OriginalPrice;
            int? totalQuantity = row.TotalQuantity;
            int? reservedQuantity = row.ReservedQuantity;

            decimal? discountPercentage = discountedPrice.HasValue && originalPrice.HasValue && originalPrice.Value > 0
                ? Math.Round((originalPrice.Value - discountedPrice.Value) / originalPrice.Value * 100, 2)
                : null;

            int? availableQuantity = totalQuantity.HasValue && reservedQuantity.HasValue
                ? totalQuantity.Value - reservedQuantity.Value
                : null;

            return new LaunchResponse(
                Id: row.Id,
                SellerId: row.SellerId,
                ProductId: row.ProductId,
                Title: row.Title,
                Description: row.Description,
                DiscountedPrice: discountedPrice,
                OriginalPrice: originalPrice,
                DiscountPercentage: discountPercentage,
                TotalQuantity: totalQuantity,
                ReservedQuantity: reservedQuantity,
                AvailableQuantity: availableQuantity,
                StartAt: row.StartAt,
                EndAt: row.EndAt,
                Status: (string)row.Status,
                CreatedOn: row.CreatedOn
            );
        }
    }
}
