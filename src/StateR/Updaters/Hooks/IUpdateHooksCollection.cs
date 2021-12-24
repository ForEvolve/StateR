using System.Threading;
using System.Threading.Tasks;

namespace StateR.Updaters.Hooks
{
    public interface IUpdateHooksCollection
    {
        Task BeforeUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
    }
}
