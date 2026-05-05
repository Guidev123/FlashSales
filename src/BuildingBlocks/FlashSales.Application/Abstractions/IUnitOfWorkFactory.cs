namespace FlashSales.Application.Abstractions
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create(Type commandType);
    }
}