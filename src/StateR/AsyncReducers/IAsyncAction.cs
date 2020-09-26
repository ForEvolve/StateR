using MediatR;

namespace StateR
{
    public interface IAsyncAction<TResponse> : IRequest<TResponse>
    {
    }
}