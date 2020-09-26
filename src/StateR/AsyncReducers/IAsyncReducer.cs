using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IAsyncReducer<TAction, TState>
        where TAction : IAction
        where TState : AsyncState
    {
        Task<TState> ReduceAsync(TAction action, TState initialState, CancellationToken cancellationToken = default);
    }
}