using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public class ReducerHandler<TState, TAction> : IActionHandler<TAction>
        where TState : StateBase
        where TAction : IAction
    {
        private readonly IEnumerable<IReducersMiddleware> _middlewares;
        private readonly IEnumerable<IReducer<TAction, TState>> _reducers;
        private readonly IState<TState> _state;

        public ReducerHandler(IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, IEnumerable<IReducersMiddleware> middlewares)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
            _middlewares = middlewares ?? throw new ArgumentNullException(nameof(middlewares));
        }

        public async Task HandleAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
        {
            foreach (var middleware in _middlewares)
            {
                await middleware.BeforeReducersAsync(context, _state, _reducers, cancellationToken);
            }
            foreach (var reducer in _reducers)
            {
                foreach (var middleware in _middlewares)
                {
                    await middleware.BeforeReducerAsync(context, _state, reducer, cancellationToken);
                }
                _state.Transform(state => reducer.Reduce(context.Action, state));
                foreach (var middleware in _middlewares)
                {
                    await middleware.AfterReducerAsync(context, _state, reducer, cancellationToken);
                }
            }
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterReducersAsync(context, _state, _reducers, cancellationToken);
                await middleware.BeforeNotifyAsync(context, _state, _reducers, cancellationToken);
            }
            _state.Notify();
            foreach (var middleware in _middlewares)
            {
                await middleware.AfterNotifyAsync(context, _state, _reducers, cancellationToken);
            }
        }
    }
}