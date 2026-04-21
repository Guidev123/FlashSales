using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using MidR.Behaviors;
using MidR.Interfaces;
using Serilog.Context;
using System.Diagnostics;

namespace Deliveryix.Commons.Application.Behaviors
{
    public sealed class RequestLoggingBehavior<TRequest, TResponse>(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger) : IRequestBehavior<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
            where TResponse : Result
    {
        public async Task<TResponse> ExecuteAsync(TRequest request, RequestDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var requestModule = GetRequestModule(typeof(TRequest).FullName!);

            Activity.Current?.SetTag("request.module", requestModule);
            Activity.Current?.SetTag("request.name", requestName);

            var stopwatch = Stopwatch.StartNew();
            using (LogContext.PushProperty("Module", requestModule))
            {
                try
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Processing request: {RequestName}", requestName);
                    }

                    var result = await next();

                    stopwatch.Stop();
                    var executionTime = stopwatch.ElapsedMilliseconds;

                    if (result.IsSuccess)
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Request: {RequestName} processed successfully in {ExecutionTimeInMilliseconds}ms",
                            requestName, executionTime);
                        }
                    }
                    else
                    {
                        using (LogContext.PushProperty("Error", result.Error, true))
                        {
                            logger.LogError("Request: {RequestName} failed in {ExecutionTimeInMilliseconds}ms",
                                requestName, executionTime);
                        }
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    var executionTime = stopwatch.ElapsedMilliseconds;

                    logger.LogError(ex, "Request: {RequestName} failed in {ExecutionTimeInMilliseconds}ms with unhandled exception", requestName, executionTime);

                    throw new FlashSalesException(typeof(TRequest).Name, innerException: ex);
                }
            }
        }

        private static string GetRequestModule(string requestName) => requestName.Split('.')[1];
    }
}