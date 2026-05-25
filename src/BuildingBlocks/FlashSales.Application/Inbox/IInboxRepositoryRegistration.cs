namespace FlashSales.Application.Inbox
{
    public interface IInboxRepositoryRegistration
    {
        bool Matches(Type commandType);

        IInboxRepository Resolve(IServiceProvider sp);
    }
}