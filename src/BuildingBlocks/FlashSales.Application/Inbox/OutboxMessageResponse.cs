namespace FlashSales.Application.Inbox
{
    public sealed class InboxMessageResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = null!;
    }
}