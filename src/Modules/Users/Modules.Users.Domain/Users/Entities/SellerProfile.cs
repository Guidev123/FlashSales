using FlashSales.Domain.DomainObjects;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Domain.Users.Entities
{
    public sealed class SellerProfile : Entity
    {
        private SellerProfile(Guid userId, Document document, PaymentAccount paymentAccount)
        {
            UserId = userId;
            Document = document;
            PaymentAccount = paymentAccount;
            Status = SellerStatus.Inactive;
            Validate();
        }

        private SellerProfile()
        { }

        public Guid UserId { get; private set; }
        public Document Document { get; private set; } = null!;
        public PaymentAccount PaymentAccount { get; private set; } = null!;
        public SellerStatus Status { get; private set; }
        public DateTimeOffset? ActivatedOn { get; private set; }

        public static SellerProfile Create(Guid userId, string document, PaymentAccount paymentAccount)
        {
            var sellerProfile = new SellerProfile(userId, document, paymentAccount);

            return sellerProfile;
        }

        public void Activate()
        {
            if (Status == SellerStatus.Active) return;
            Status = SellerStatus.Active;
            ActivatedOn = DateTimeOffset.UtcNow;
            AddDomainEvent(SellerActivatedDomainEvent.Create(UserId, Id));
        }

        protected override void Validate()
        {
        }
    }
}