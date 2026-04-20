using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Extensions
{
    public static class ExceptionExtensions
    {
        public static string? GetExceptionMessage(this Exception? exception)
        {
            if (exception is null)
                return null;

            return exception switch
            {
                FlashSalesException flshsEx when flshsEx.Error?.Description is not null => flshsEx.Error.Description,
                _ when exception.InnerException?.Message is not null => exception.InnerException.Message,
                _ => exception.Message
            };
        }
    }
}