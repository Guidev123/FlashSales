using FlashSales.Domain.DomainObjects;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Domain.Launches.Entities
{
    public sealed class StockReservation : Entity
    {
        private StockReservation()
        { }

        private StockReservation(Guid launchId, Guid orderId, int quantity)
        {
            LaunchId = launchId;
            OrderId = orderId;
            Quantity = quantity;
            Validate();
        }

        public Guid LaunchId { get; private set; }
        public Guid OrderId { get; private set; }
        public int Quantity { get; private set; }

        public static StockReservation Create(Guid launchId, Guid orderId, int quantity)
        {
            var reservation = new StockReservation(launchId, orderId, quantity);

            return reservation;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(LaunchId != Guid.Empty, LaunchErrors.LaunchIdCannotBeEmpty.Description);
            AssertionConcern.EnsureTrue(OrderId != Guid.Empty, LaunchErrors.OrderIdCannotBeEmpty.Description);
            AssertionConcern.EnsureTrue(Quantity >= 1, LaunchErrors.QuantityMustBeAtLeastOne.Description);
        }
    }
}