using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Domain.Users.ValueObjects
{
    public sealed record PaymentAccount : ValueObject
    {
        private PaymentAccount(string bankCode, string agency, string number, BankAccountType type)
        {
            BankCode = bankCode;
            Agency = agency;
            AccountNumber = number;
            AccountType = type;
            Validate();
        }

        private PaymentAccount()
        { }

        public string BankCode { get; } = string.Empty;
        public string Agency { get; } = string.Empty;
        public string AccountNumber { get; } = string.Empty;
        public BankAccountType AccountType { get; }

        public static PaymentAccount Create(
            string bankCode,
            string agency,
            string number,
            BankAccountType type
            )
        {
            return new PaymentAccount(bankCode, agency, number, type);
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureNotEmpty(BankCode, SellerErrors.BankCodeMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotEmpty(Agency, SellerErrors.AgencyMustBeNotEmpty.Description);
            AssertionConcern.EnsureNotEmpty(AccountNumber, SellerErrors.AccountNumberMustBeNotEmpty.Description);
            AssertionConcern.EnsureMaxLength(BankCode, 3, SellerErrors.BankCodeTooLong.Description);
            AssertionConcern.EnsureMaxLength(Agency, 10, SellerErrors.AgencyTooLong.Description);
            AssertionConcern.EnsureMaxLength(AccountNumber, 20, SellerErrors.AccountNumberTooLong.Description);
        }
    }
}