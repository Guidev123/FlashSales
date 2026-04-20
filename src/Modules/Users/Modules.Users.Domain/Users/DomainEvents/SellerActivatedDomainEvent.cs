using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record SellerActivatedDomainEvent : DomainEvent
    {
        public static SellerActivatedDomainEvent Create(Guid userId, Guid sellerId)
        {
            return new SellerActivatedDomainEvent(userId, sellerId);
        }

        private SellerActivatedDomainEvent(Guid userId, Guid sellerId)
            : base(userId, nameof(SellerActivatedDomainEvent))
        {
            UserId = userId;
            SellerId = sellerId;
        }

        private SellerActivatedDomainEvent()
        { }

        public Guid UserId { get; set; }
        public Guid SellerId { get; set; }
    }
}