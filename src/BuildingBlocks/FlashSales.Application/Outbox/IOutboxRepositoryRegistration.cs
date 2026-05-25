namespace FlashSales.Application.Outbox
{
    public interface IOutboxRepositoryRegistration
    {
        bool Matches(Type commandType);

        IOutboxRepository Resolve(IServiceProvider sp);
    }
}