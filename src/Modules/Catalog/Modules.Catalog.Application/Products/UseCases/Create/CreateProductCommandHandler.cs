using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Catalog.Domain.Products.Entities;
using Modules.Catalog.Domain.Products.Errors;
using Modules.Catalog.Domain.Products.Repositories;
using Modules.Catalog.Domain.Sellers.Errors;
using Modules.Catalog.Domain.Sellers.Repositories;

namespace Modules.Catalog.Application.Products.UseCases.Create
{
    internal sealed class CreateProductCommandHandler(
        ISellerRepository sellerRepository,
        IProductRepository productRepository
        ) : ICommandHandler<CreateProductCommand, CreateProductResponse>
    {
        public async Task<Result<CreateProductResponse>> ExecuteAsync(CreateProductCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await sellerRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            if(seller is null)
            {
                return Result.Failure<CreateProductResponse>(SellerErrors.NotFound(request.UserId));
            }

            var category = await productRepository.GetCategoryByIdAsync(request.CategoryId, cancellationToken);
            if(category is null)
            {
                return Result.Failure<CreateProductResponse>(CategoryErrors.NotFound(request.CategoryId));
            }

            var product = Product.Create(
                seller.Id,
                category.Id,
                request.Name,
                request.Description
                );

            productRepository.Add(product);

            return new CreateProductResponse(product.Id);
        }
    }
}
