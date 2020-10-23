using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers.Hooks
{
    public interface IBeforeReducerHook
    {
        Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
    }
}
