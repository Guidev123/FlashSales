using FlashSales.Domain.Results;
using MidR.Interfaces;

namespace FlashSales.Application.Messaging
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
      where TQuery : IQuery<TResponse>;
}