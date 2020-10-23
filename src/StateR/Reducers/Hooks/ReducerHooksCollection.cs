using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers.Hooks
{
    public class ReducerHooksCollection : IReducerHooksCollection
    {
        private readonly IEnumerable<IBeforeReducerHook> _beforeReducerHooks;
        private readonly IEnumerable<IAfterReducerHook> _afterReducerHooks;
        public ReducerHooksCollection(IEnumerable<IBeforeReducerHook> beforeReducerHooks, IEnumerable<IAfterReducerHook> afterReducerHooks)
        {
            _beforeReducerHooks = beforeReducerHooks ?? throw new ArgumentNullException(nameof(beforeReducerHooks));
            _afterReducerHooks = afterReducerHooks ?? throw new ArgumentNullException(nameof(afterReducerHooks));
        }

        public async Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            foreach (var hook in _beforeReducerHooks)
            {
                await hook.BeforeReducerAsync(context, state, reducer, cancellationToken);
            }
        }

        public async Task AfterReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase
        {
            foreach (var hook in _afterReducerHooks)
            {
                await hook.AfterReducerAsync(context, state, reducer, cancellationToken);
            }
        }
    }
}
