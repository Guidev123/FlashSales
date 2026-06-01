namespace FlashSales.Application.Outbox
{
    public interface IOutboxBatchProcessor
    {
        Task ProcessBatchAsync(CancellationToken cancellationToken = default);
    }
}
