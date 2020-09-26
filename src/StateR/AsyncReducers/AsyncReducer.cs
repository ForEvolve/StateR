using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public abstract class AsyncReducer<TAction, TState, TResponse> : IAsyncReducer<TAction, TState, TResponse>, IReducer<TState, OperationStateUpdated>
        where TAction : IAsyncAction<TResponse>
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
                    await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Loading), cancellationToken);
                    var response = await LoadAsync(action, initialState);
                    var completedAction = CreateCompletedAction(response);
                    await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Succeeded), cancellationToken);
                    await Store.DispatchAsync(completedAction, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await Store.DispatchAsync(new OperationStateUpdated(AsyncOperationState.Failed), cancellationToken);
                var errorState = new AsyncErrorState<TAction, TState, TResponse>(action, initialState, ex);
                var errorAction = new AsyncErrorOccured<TAction, TState, TResponse>(errorState);
                await Store.DispatchAsync(errorAction, cancellationToken);
            }
        }

        protected abstract Task<TResponse> LoadAsync(TAction action, TState initalState, CancellationToken cancellationToken = default);
        protected abstract IAction CreateCompletedAction(TResponse response);

        public virtual TState Reduce(OperationStateUpdated action, TState initialState)
            => initialState with { RecordState = action.NewRecordState };
    }

    public class AsyncReducerHandler<TState, TAction, TResponse> : IRequestHandler<TAction, TResponse>
        where TAction : IAsyncAction<TResponse>
        where TState : AsyncState

    {
        private readonly IEnumerable<IAsyncReducer<TAction, TState, TResponse>> _reducers;
        private readonly IState<TState> _state;

        public AsyncReducerHandler(IEnumerable<IAsyncReducer<TAction, TState, TResponse>> reducers, IState<TState> state)
        {
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public async Task<TResponse> Handle(TAction action, CancellationToken cancellationToken)
        {
            foreach (var reducer in _reducers)
            {
                await reducer.ReduceAsync(action, _state.Current, cancellationToken);
            }
            _state.Notify();
            return default;
        }
    }
}