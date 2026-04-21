using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
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

        public async Task<TResponse> ExecuteAsync(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Transaction started for request {RequestType}", typeof(TRequest).Name);
            }

            try
            {
                var response = await next();

                if (response.IsFailure)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Transaction rollback performed due to handler failure");
                    }

                    await unitOfWork.RollbackAsync(cancellationToken);
                    return response;
                }

                var events = domainEventCollector.Flush();

                var tasks = events.Select(domainEvent =>
                {
                    return publisher.PublishToBusAsync(domainEvent, cancellationToken);
                });

                var isSucces = await unitOfWork.CommitAsync(cancellationToken);

                if (!isSucces)
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                    {
                        logger.LogWarning("Transaction failed to commit changes");
                    }

                    return (TResponse)Result.Failure(TransactionFailedError);
                }

                await Task.WhenAll(tasks);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Transaction completed successfully");
                }

                return response;
            }
            catch
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Transaction rollback performed due to exception");
                }

                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}