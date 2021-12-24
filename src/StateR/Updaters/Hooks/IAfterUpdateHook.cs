using System.Threading;
using System.Threading.Tasks;

namespace StateR.Updater.Hooks
{
    public interface IAfterUpdateHook
    {
        Task AfterUpdateAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IUpdater<TAction, TState> updater, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
    }
}
