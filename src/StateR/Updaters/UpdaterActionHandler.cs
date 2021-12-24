using StateR.ActionHandlers;
using StateR.Updaters.Hooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Updaters
{
    public class UpdaterActionHandler<TState, TAction> : IActionHandler<TAction>
        where TState : StateBase
        where TAction : IAction
    {
        private readonly IUpdateHooksCollection _hooks;
        private readonly IEnumerable<IUpdater<TAction, TState>> _updaters;
        private readonly IState<TState> _state;

        public UpdaterActionHandler(IState<TState> state, IEnumerable<IUpdater<TAction, TState>> updaters, IUpdateHooksCollection hooks)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _updaters = updaters ?? throw new ArgumentNullException(nameof(updaters));
            _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
        }

        public async Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
        {
            foreach (var updater in _updaters)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await _hooks.BeforeUpdateAsync(context, _state, updater, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                _state.Set(updater.Update(context.Action, _state.Current));
                await _hooks.AfterUpdateAsync(context, _state, updater, cancellationToken);
            }
            _state.Notify();
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}