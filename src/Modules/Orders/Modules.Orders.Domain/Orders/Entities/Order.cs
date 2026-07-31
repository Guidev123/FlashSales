using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Orders.DomainEvents;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Errors;

namespace Modules.Orders.Domain.Orders.Entities
{
    public sealed class Order : Entity, IAggregateRoot
    {
        public const int PaymentWindowMinutes = 10;

        private Order(
            Guid customerId,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            int quantity,
            decimal unitPrice
            )
        {
            CustomerId = customerId;
            LaunchId = launchId;
            SellerId = sellerId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalAmount = quantity * unitPrice;
            OrderCode = $"ORD-{Id.ToString("N")[..8].ToUpperInvariant()}";
            Status = OrderStatus.AwaitingPayment;
            ExpiresAt = CreatedOn.AddMinutes(PaymentWindowMinutes);
            Validate();
        }

        private Order()
        { }

        public Guid CustomerId { get; private set; }
        public Guid LaunchId { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string OrderCode { get; private set; } = null!;
        public OrderStatus Status { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? ConfirmedAt { get; private set; }
        public string? Reason { get; private set; }

        public static Order Create(
            Guid customerId,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            int quantity,
            decimal unitPrice
            )
        {
            var order = new Order(customerId, launchId, sellerId, productId, quantity, unitPrice);

            order.AddDomainEvent(OrderCreatedDomainEvent.Create(order.Id, customerId, launchId, quantity));

            return order;
        }

        public Result MarkPaymentProcessing()
        {
            if (Status == OrderStatus.PaymentProcessing) return Result.Success();

            if (Status != OrderStatus.AwaitingPayment)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.PaymentProcessing.ToString()));

            Status = OrderStatus.PaymentProcessing;

            return Result.Success();
        }

        public Result Confirm()
        {
            if (Status == OrderStatus.Confirmed) return Result.Success();

            if (Status == OrderStatus.Cancelled || Status == OrderStatus.Refunded)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.Confirmed.ToString()));

            Status = OrderStatus.Confirmed;
            ConfirmedAt = DateTimeOffset.UtcNow;

            AddDomainEvent(OrderConfirmedDomainEvent.Create(Id, CustomerId, LaunchId, Quantity));

            return Result.Success();
        }

        public Result Cancel(string reason)
        {
            if (Status == OrderStatus.Cancelled) return Result.Success();

            if (Status == OrderStatus.Confirmed || Status == OrderStatus.Refunded)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.Cancelled.ToString()));

            Status = OrderStatus.Cancelled;
            Reason = reason;

            AddDomainEvent(OrderCancelledDomainEvent.Create(Id, CustomerId, LaunchId, Quantity, reason));

            return Result.Success();
        }

        public Result MarkExpired() => Cancel("Payment window expired");

        public Result Refund(string reason)
        {
            if (Status == OrderStatus.Refunded) return Result.Success();

            Status = OrderStatus.Refunded;
            Reason = reason;

            AddDomainEvent(OrderRefundedDomainEvent.Create(Id, CustomerId, LaunchId, Quantity, reason));

            return Result.Success();
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(CustomerId != Guid.Empty, OrderErrors.CustomerIdRequired.Description);
            AssertionConcern.EnsureTrue(LaunchId != Guid.Empty, OrderErrors.LaunchIdRequired.Description);
            AssertionConcern.EnsureGreaterThan(Quantity, 0, OrderErrors.QuantityMustBeAtLeastOne.Description);
            AssertionConcern.EnsureGreaterThan(UnitPrice, 0, OrderErrors.UnitPriceMustBeGreaterThanZero.Description);
        }
    }
}
