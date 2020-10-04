using System.Threading;
using System.Threading.Tasks;

namespace StateR.Old
{
    public interface IAsyncReducer<TAction, TState>
        where TAction : IAction
        where TState : AsyncState
    {
        Task ReduceAsync(TAction action, TState initialState, CancellationToken cancellationToken = default);
    }
}