namespace FlashSales.Application.Abstractions
{
    public interface IUnitOfWorkRegistration
    {
        bool Matches(Type commandType);

        IUnitOfWork Resolve(IServiceProvider sp);
    }
}