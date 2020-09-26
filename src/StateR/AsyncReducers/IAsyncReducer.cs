using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IAsyncReducer<TAction, TState, TResponse>
        where TAction : IAsyncAction<TResponse>
        where TState : AsyncState
    {
        Task ReduceAsync(TAction action, TState initialState, CancellationToken cancellationToken = default);
    }
}