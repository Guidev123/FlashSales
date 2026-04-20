using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Abstractions
{
    public interface IRepository<TAggregateRoot>
        where TAggregateRoot : IAggregateRoot;
}