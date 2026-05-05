using Dapper;
using Modules.Catalog.Application.Abstractions;
using Modules.Catalog.Application.Products.Dtos;
using Modules.Catalog.Application.Products.Services;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class ProductQueryService(
        ICatalogUnitOfWork unitOfWork
        ) : IProductQueryService
    {
        public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync(int page, int size, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    p."Id",
                    p."Name",
                    p."Description",
                    p."Status",
                    c."Id" AS "CategoryId",
                    c."Name" AS "CategoryName",
                    pi."Id" AS "ImageId",
                    pi."ProductId" AS "ImageProductId",
                    pi."Url",
                    pi."Order",
                    pi."IsCover"
                FROM catalog."Products" p
                INNER JOIN catalog."Categories" c ON c."Id" = p."CategoryId"
                LEFT JOIN catalog."ProductImages" pi ON pi."ProductId" = p."Id"
                ORDER BY p."CreatedOn" DESC
                OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY
            """;

            var productMap = new Dictionary<Guid, ProductResponse>();

            await unitOfWork.Connection.QueryAsync(
                sql,
                types:
                [
                    typeof(ProductFlat),
                    typeof(CategoryResponse),
                    typeof(ProductImageResponse)
                ],
                map: objects =>
                {
                    var flat = (ProductFlat)objects[0];
                    var category = (CategoryResponse)objects[1];
                    var image = (ProductImageResponse?)objects[2];

                    if (!productMap.TryGetValue(flat.Id, out var product))
                    {
                        product = new ProductResponse(flat.Id, flat.Name, flat.Description, [], category);
                        productMap[flat.Id] = product;
                    }

                    if (image is not null)
                        product.Images.Add(image);

                    return product;
                },
                param: new { Offset = (page - 1) * size, Size = size },
                splitOn: "CategoryId,ImageId"
            ).WaitAsync(cancellationToken);

            return productMap.Values.ToList().AsReadOnly();
        }

        public async Task<ProductResponse?> GetAsync(Guid productId, CancellationToken cancellationToken = default)
        {
            const string sql = """
                SELECT
                    p."Id",
                    p."Name",
                    p."Description",
                    p."CategoryId",
                    c."Name" AS "CategoryName",
                    pi."Id" AS "ImageId",
                    pi."ProductId",
                    pi."Url",
                    pi."Order",
                    pi."IsCover"
                FROM catalog."Products" p
                INNER JOIN catalog."Categories" c ON c."Id" = p."CategoryId"
                LEFT JOIN catalog."ProductImages" pi ON pi."ProductId" = p."Id"
                WHERE p."Id" = @productId
                ORDER BY pi."Order";
                """;

            var rows = await unitOfWork.Connection.QueryAsync(sql, new { productId });

            var first = rows.FirstOrDefault();
            if (first is null) return null;

            return new(
                first.Id,
                first.Name,
                first.Description,
                rows.Where(r => r.ImageId is not null)
                    .Select(r => new ProductImageResponse(
                        r.ImageId,
                        r.ProductId,
                        r.Url,
                        r.Order,
                        r.IsCover
                    )).ToList(),
                new CategoryResponse(first.CategoryId, first.CategoryName)
            );
        }

        public Task<IReadOnlyCollection<CategoryResponse>> GetCategoriesAsync(int page, int size, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCategoryTotalCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}