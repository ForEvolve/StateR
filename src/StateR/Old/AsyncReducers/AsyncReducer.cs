using StateR.AfterEffects;
using StateR.Reducers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public abstract class AsyncReducer<TAction, TState> : IAsyncReducer<TAction, TState>, IReducer<OperationStateUpdated, TState>
        where TAction : IAction
        where TState : AsyncState
    {
        public AsyncReducer(IStore store)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        protected IStore Store { get; }

        public async Task ReduceAsync(TAction action, TState initialState, CancellationToken cancellationToken = default)
        {
            try
            {
                if (initialState.RecordState == AsyncOperationState.Idle)
                {
                    const bool configureAwait = true;
                    await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Loading), cancellationToken).ConfigureAwait(configureAwait);
                    var completedAction = await LoadAsync(action, initialState).ConfigureAwait(configureAwait);
                    await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Succeeded), cancellationToken).ConfigureAwait(configureAwait);
                    await Store.DispatchAsync(completedAction, cancellationToken).ConfigureAwait(configureAwait);
                }
            }
            catch (Exception ex)
            {
                await ExceptionCatchedAsync(ex, cancellationToken);
                await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Failed), cancellationToken);
                var errorAction = new AsyncErrorOccured(action, initialState, Store.GetState<TState>(), ex);
                await Store.DispatchAsync(errorAction, cancellationToken);
                await ExceptionDispatchedAsync(ex, cancellationToken);
            }
        }

        protected virtual Task ExceptionCatchedAsync(Exception exception, CancellationToken cancellationToken = default) => Task.CompletedTask;
        protected virtual Task ExceptionDispatchedAsync(Exception exception, CancellationToken cancellationToken = default) => Task.CompletedTask;

        protected abstract Task<IAction> LoadAsync(TAction action, TState initalState, CancellationToken cancellationToken = default);

        public virtual TState Reduce(OperationStateUpdated action, TState initialState)
            => initialState with { RecordState = action.NewRecordState };
    }

    public class AsyncReducerHandler<TState, TAction> : IActionAfterEffects<TAction>
        where TAction : IAction
        where TState : AsyncState

    {
        private readonly IEnumerable<IAsyncReducer<TAction, TState>> _reducers;
        private readonly IState<TState> _state;

        public AsyncReducerHandler(IEnumerable<IAsyncReducer<TAction, TState>> reducers, IState<TState> state)
        {
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public async Task HandleAfterEffectAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
        {
            foreach (var reducer in _reducers)
            {
                await reducer.ReduceAsync(context.Action, _state.Current, cancellationToken);
            }
        }
    }
}