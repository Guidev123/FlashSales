using FlashSales.Domain.Results;
using MidR.Interfaces;

namespace FlashSales.Application.Messaging
{
    public interface IQuery<TR> : IRequest<Result<TR>>;
}