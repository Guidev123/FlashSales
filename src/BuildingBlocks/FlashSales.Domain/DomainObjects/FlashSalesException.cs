using FlashSales.Domain.Results;

namespace FlashSales.Domain.DomainObjects
{
    public class FlashSalesException : Exception
    {
        public FlashSalesException(string requestName, Error? error = default, Exception? innerException = default)
            : base("Application exception", innerException)
        {
            RequestName = requestName;
            Error = error;
        }

        public string RequestName { get; }

        public Error? Error { get; }
    }
}