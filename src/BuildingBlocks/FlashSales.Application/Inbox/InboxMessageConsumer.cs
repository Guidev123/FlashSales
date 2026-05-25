namespace FlashSales.Application.Inbox
{
    public sealed class InboxMessageConsumer
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid InboxMessageId { get; set; }
        public string Name { get; set; } = null!;
    }
}