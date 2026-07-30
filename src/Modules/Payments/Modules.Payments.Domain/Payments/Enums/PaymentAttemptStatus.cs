namespace Modules.Payments.Domain.Payments.Enums
{
    public enum PaymentAttemptStatus
    {
        None,
        Initiated,
        Authorized,
        Declined,
        TimedOut
    }
}
