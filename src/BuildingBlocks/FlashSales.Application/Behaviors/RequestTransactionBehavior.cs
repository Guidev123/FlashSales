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
         IUnitOfWorkFactory unitOfWorkFactory,
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
            if (request is ITransactionalLessCommand) return await next();

            var unitOfWork = unitOfWorkFactory.Create(typeof(TRequest));

            var isOutermost = await BeginTransactionAsync(unitOfWork, cancellationToken);

            try
            {
                var response = await next();

                return response.IsFailure
                    ? await HandleFailureAsync(unitOfWork, isOutermost, response, cancellationToken)
                    : await CommitTransactionAsync(unitOfWork, isOutermost, response, cancellationToken);
            }
            catch
            {
                if (isOutermost)
                {
                    await unitOfWork.RollbackAsync(cancellationToken);
                }

                throw;
            }
        }

        private async Task<bool> BeginTransactionAsync(
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken)
        {
            if (unitOfWork.Transaction is not null) return false;

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Transaction started for {RequestType}", typeof(TRequest).Name);
            }

            return true;
        }

        private async Task<TResponse> CommitTransactionAsync(
            IUnitOfWork unitOfWork,
            bool isOutermost,
            TResponse response,
            CancellationToken cancellationToken)
        {
            if (!isOutermost) return response;

            var events = domainEventCollector.Flush();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var domainEvent in events)
            {
                await publisher.PublishAsync(domainEvent, cancellationToken);
            }

            var success = await unitOfWork.CommitAsync(cancellationToken);
            if (!success)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Commit failed for {RequestType}", typeof(TRequest).Name);
                }

                return (TResponse)Result.Failure(TransactionFailedError);
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Transaction completed for {RequestType}", typeof(TRequest).Name);
            }

            return response;
        }

        private async Task<TResponse> HandleFailureAsync(
            IUnitOfWork unitOfWork,
            bool isOutermost,
            TResponse response,
            CancellationToken cancellationToken)
        {
            if (!isOutermost) return response;

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Rollback due to handler failure for {RequestType}",
                    typeof(TRequest).Name);
            }

            await unitOfWork.RollbackAsync(cancellationToken);

            return response;
        }
    }
}