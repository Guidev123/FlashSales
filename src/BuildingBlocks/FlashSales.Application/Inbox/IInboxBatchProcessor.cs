namespace FlashSales.Application.Inbox
{
    public interface IInboxBatchProcessor
    {
        Task ProcessBatchAsync(CancellationToken cancellationToken = default);
    }
}
