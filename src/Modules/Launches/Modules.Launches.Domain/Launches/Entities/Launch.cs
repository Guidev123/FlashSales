using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Launches.Domain.Launches.DomainEvents;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.ValueObjects;

namespace Modules.Launches.Domain.Launches.Entities
{
    public sealed class Launch : Entity, IAggregateRoot
    {
        private readonly List<StockReservation> _stockReservations = [];

        private Launch(Guid sellerId, Guid productId, string title, string description)
        {
            SellerId = sellerId;
            ProductId = productId;
            Metadata = LaunchMetadata.Create(title, description);
            Status = LaunchStatus.Draft;
            Validate();
        }

        private Launch()
        { }

        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public LaunchMetadata Metadata { get; private set; } = null!;
        public LaunchPrice? Price { get; private set; }
        public LaunchStock? Stock { get; private set; }
        public LaunchSchedule? Schedule { get; private set; }
        public LaunchStatus Status { get; private set; }
        public IReadOnlyCollection<StockReservation> StockReservations => _stockReservations.AsReadOnly();

        public static Launch Create(Guid sellerId, Guid productId, string title, string description)
        {
            var launch = new Launch(sellerId, productId, title, description);

            launch.AddDomainEvent(LaunchCreatedDomainEvent.Create(launch.Id, sellerId, productId, title));

            return launch;
        }

        public Result SetSchedule(LaunchPrice price, LaunchStock stock, LaunchSchedule schedule)
        {
            if (Status != LaunchStatus.Draft)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), LaunchStatus.Scheduled.ToString()));

            if (schedule.StartAt <= DateTimeOffset.UtcNow)
                return Result.Failure(LaunchErrors.InvalidSchedule);

            Price = price;
            Stock = stock;
            Schedule = schedule;
            Status = LaunchStatus.Scheduled;

            AddDomainEvent(LaunchScheduledDomainEvent.Create(
                Id,
                price.DiscountedPrice,
                price.OriginalPrice,
                stock.TotalQuantity,
                schedule.StartAt,
                schedule.EndAt));

            return Result.Success();
        }

        public Result Activate()
        {
            if (Status != LaunchStatus.Scheduled)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), LaunchStatus.Active.ToString()));

            Status = LaunchStatus.Active;

            AddDomainEvent(LaunchActivatedDomainEvent.Create(
                Id,
                SellerId,
                ProductId,
                Metadata.Title,
                Price!.DiscountedPrice,
                Price.OriginalPrice,
                Stock!.TotalQuantity,
                Schedule!.StartAt,
                Schedule.EndAt));

            return Result.Success();
        }

        public Result End()
        {
            if (Status != LaunchStatus.Active)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), LaunchStatus.Ended.ToString()));

            Status = LaunchStatus.Ended;

            AddDomainEvent(LaunchEndedDomainEvent.Create(Id, Stock!.ReservedQuantity));

            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status == LaunchStatus.Active || Status == LaunchStatus.Ended || Status == LaunchStatus.SoldOut)
                return Result.Failure(LaunchErrors.CannotCancelAfterActivation);

            if (Status != LaunchStatus.Draft && Status != LaunchStatus.Scheduled)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), LaunchStatus.Cancelled.ToString()));

            Status = LaunchStatus.Cancelled;

            AddDomainEvent(LaunchCancelledDomainEvent.Create(Id, SellerId));

            return Result.Success();
        }

        public Result ReserveStock(int quantity, Guid orderId)
        {
            if (Status != LaunchStatus.Active)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), "ReserveStock"));

            if (_stockReservations.Any(r => r.OrderId == orderId))
                return Result.Success();

            if (Stock!.AvailableQuantity < quantity)
                return Result.Failure(LaunchErrors.InsufficientStock);

            Stock = LaunchStock.Create(Stock.TotalQuantity, Stock.ReservedQuantity + quantity);
            _stockReservations.Add(StockReservation.Create(Id, orderId, quantity));

            AddDomainEvent(StockReservedDomainEvent.Create(Id, quantity, orderId));

            if (Stock.AvailableQuantity == 0)
            {
                Status = LaunchStatus.SoldOut;
                AddDomainEvent(LaunchSoldOutDomainEvent.Create(Id, Stock.ReservedQuantity));
            }

            return Result.Success();
        }

        public Result ReleaseStock(int quantity, Guid orderId)
        {
            if (Status != LaunchStatus.SoldOut && Status != LaunchStatus.Active)
                return Result.Failure(LaunchErrors.InvalidStatusTransition(Status.ToString(), "ReleaseStock"));

            var reservation = _stockReservations.FirstOrDefault(r => r.OrderId == orderId);

            if (reservation is null)
                return Result.Success();

            _stockReservations.Remove(reservation);
            Stock = LaunchStock.Create(Stock!.TotalQuantity, Stock.ReservedQuantity - quantity);

            if (Status == LaunchStatus.SoldOut)
                Status = LaunchStatus.Active;

            AddDomainEvent(StockReleasedDomainEvent.Create(Id, quantity, orderId));

            return Result.Success();
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, LaunchErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureTrue(ProductId != Guid.Empty, LaunchErrors.ProductIdRequired.Description);
            AssertionConcern.EnsureNotNull(Metadata, LaunchErrors.TitleRequired.Description);
        }
    }
}