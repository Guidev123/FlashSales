using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using MidR.Behaviors;
using MidR.Interfaces;

namespace FlashSales.Application.Behaviors
{
    public sealed class RequestTransactionBehavior<TRequest, TResponse>(
        IDomainEventCollector domainEventCollector,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ILogger<RequestTransactionBehavior<TRequest, TResponse>> logger
        ) : IRequestBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, IBaseCommand
        where TResponse : Result
    {
        private static readonly Error TransactionFailedError = Error.Problem(
            "Database.TransactionFailedError",
            "Failed to commit transaction");

        public async Task<TResponse> ExecuteAsync(
            TRequest request,
            RequestDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Transaction started for {RequestType}", typeof(TRequest).Name);
            }

            try
            {
                var response = await next();

                if (response.IsFailure)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Rollback due to handler failure for {RequestType}", typeof(TRequest).Name);
                    }
                    await unitOfWork.RollbackAsync(cancellationToken);
                    return response;
                }

                var events = domainEventCollector.Flush();

                await unitOfWork.SaveChangesAsync(cancellationToken);

                foreach (var domainEvent in events)
                {
                    await publisher.PublishAsync(domainEvent, cancellationToken);
                }

                var success = await unitOfWork.CommitAsync(cancellationToken);

                if (!success)
                {
                    logger.LogWarning("Commit failed for {RequestType}", typeof(TRequest).Name);
                    return (TResponse)Result.Failure(TransactionFailedError);
                }

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Transaction completed for {RequestType}", typeof(TRequest).Name);
                }

                return response;
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}