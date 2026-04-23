using FlashSales.Domain.DomainObjects;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
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
            Status = SellerStatus.Active;
            ActivatedOn = DateTimeOffset.UtcNow;
            Validate();
        }

        private SellerProfile()
        { }

        public Guid UserId { get; private set; }
        public Document Document { get; private set; } = null!;
        public PaymentAccount PaymentAccount { get; private set; } = null!;
        public SellerStatus Status { get; private set; }
        public DateTimeOffset? ActivatedOn { get; private set; }
        public string? ProfilePictureUrl { get; private set; }

        public static SellerProfile Create(Guid userId, string document, PaymentAccount paymentAccount)
        {
            var sellerProfile = new SellerProfile(userId, document, paymentAccount);

            sellerProfile.AddDomainEvent(SellerActivatedDomainEvent.Create(sellerProfile.UserId, sellerProfile.Id));

            return sellerProfile;
        }

        public void SetProfilePictureUrl(string profilePictureUrl)
        {
            ProfilePictureUrl = profilePictureUrl;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(UserId != Guid.Empty, SellerErrors.UserIdMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotNull(Document, SellerErrors.DocumentMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotNull(PaymentAccount, SellerErrors.AccountNumberMustBeNotEmpty.Description);
        }
    }
}