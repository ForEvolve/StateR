using StateR.ActionHandlers;
using StateR.Reducers.Hooks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers
{
    public class ReducerHandler<TState, TAction> : IActionHandler<TAction>
        where TState : StateBase
        where TAction : IAction
    {
        private readonly IReducerHooksCollection _hooks;
        private readonly IEnumerable<IReducer<TAction, TState>> _reducers;
        private readonly IState<TState> _state;

        public ReducerHandler(IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, IReducerHooksCollection hooks)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
            _hooks = hooks ?? throw new ArgumentNullException(nameof(hooks));
        }

        public async Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
        {
            foreach (var reducer in _reducers)
            {
                await _hooks.BeforeReducerAsync(context, _state, reducer, cancellationToken);
                _state.Set(reducer.Reduce(context.Action, _state.Current));
                await _hooks.AfterReducerAsync(context, _state, reducer, cancellationToken);
            }
            _state.Notify();
        }
    }
}