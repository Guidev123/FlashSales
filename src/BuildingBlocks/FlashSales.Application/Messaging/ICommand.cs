using FlashSales.Domain.Results;
using MidR.Interfaces;

namespace FlashSales.Application.Messaging
{
    public interface ICommand : IRequest<Result>, IBaseCommand;

    public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

    public interface IBaseCommand;
}