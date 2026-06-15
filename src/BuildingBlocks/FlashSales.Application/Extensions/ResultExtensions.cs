using FlashSales.Application.Abstractions;
using FlashSales.Domain.Results;

namespace FlashSales.Application.Extensions
{
    public static class ResultExtensions
    {
        public static bool IsAlreadyExistsError(this Result result) =>
            result.IsFailure &&
            (result.Error!.Code == CommitResult.UniqueViolationCode ||
             result.Error!.Code.EndsWith(".AlreadyExists", StringComparison.Ordinal));
    }
}