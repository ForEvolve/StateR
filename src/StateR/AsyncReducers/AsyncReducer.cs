using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public abstract class AsyncReducer<TAction, TState> : IAsyncReducer<TAction, TState>, IReducer<TState, OperationStateUpdated>
        where TAction : IAction
        where TState : AsyncState
    {
        private readonly IStore _store;
        public AsyncReducer(IStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<TState> ReduceAsync(TAction action, TState initialState, CancellationToken cancellationToken = default)
        {
            try
            {
                if (initialState.RecordState == AsyncOperationState.Idle)
                {
                    await _store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Loading), cancellationToken);
                    var updatedState = await LoadAsync(action, initialState);
                    var completedAction = CreateCompletedAction(action, initialState, updatedState);
                    await _store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Succeeded), cancellationToken);
                    await _store.DispatchAsync(completedAction, cancellationToken);
                    return updatedState;
                }
            }
            catch (Exception ex)
            {
                await _store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Failed), cancellationToken);
                var errorState = new AsyncErrorState<TAction, TState>(action, initialState, ex);
                var errorAction = new AsyncErrorOccured<TAction, TState>(errorState);
                await _store.DispatchAsync(errorAction, cancellationToken);
            }
            return initialState;
        }

        protected abstract Task<TState> LoadAsync(TAction action, TState initalState);
        protected abstract IAction CreateCompletedAction(TAction action, TState initalState, TState updatedState);

        public virtual TState Reduce(OperationStateUpdated action, TState initialState)
            => initialState with { RecordState = action.NewRecordState };
    }

    public class AsyncReducerHandler<TState, TAction> : IRequestHandler<TAction>
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

        public async Task<Unit> Handle(TAction action, CancellationToken cancellationToken)
        {
            foreach (var reducer in _reducers)
            {
                var updatedState = await reducer.ReduceAsync(action, _state.Current, cancellationToken);
                _state.Set(updatedState);
            }
            _state.Notify();
            return Unit.Value;
        }
    }
}