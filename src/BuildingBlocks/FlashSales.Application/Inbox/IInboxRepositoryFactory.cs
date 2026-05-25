namespace FlashSales.Application.Inbox
{
    public interface IInboxRepositoryFactory
    {
        IInboxRepository Create(Type commandType);
    }
}