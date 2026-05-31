using FlashSales.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Application;
using Modules.Catalog.Application.Abstractions;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class CatalogUnitOfWorkRegistration : IUnitOfWorkRegistration
    {
        public bool Matches(Type commandType)
            => CatalogModule.Assemblies.Contains(commandType.Assembly);

        public IUnitOfWork Resolve(IServiceProvider sp)
            => sp.GetRequiredService<ICatalogUnitOfWork>();
    }
}