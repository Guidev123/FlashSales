namespace FlashSales.Application.Outbox
{
    public sealed class OutboxMessageConsumer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OutboxMessageId { get; set; }
        public string Name { get; set; } = null!;
    }
}