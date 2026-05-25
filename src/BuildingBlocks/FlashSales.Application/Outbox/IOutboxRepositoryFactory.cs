namespace FlashSales.Application.Outbox
{
    public interface IOutboxRepositoryFactory
    {
        IOutboxRepository Create(Type commandType);
    }
}