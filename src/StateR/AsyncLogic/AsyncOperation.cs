using Microsoft.Extensions.DependencyInjection;
using StateR.AfterEffects;
using StateR.Reducers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.AsyncLogic
{
    public abstract class AsyncOperation<TAction, TState, TSuccessAction> : IActionAfterEffects<TAction>, IReducer<StatusUpdated<TState>, TState>
        where TAction : IAction
        where TState : AsyncState
        where TSuccessAction : IAction
    {
        public AsyncOperation(IStore store)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        protected IStore Store { get; }

        public virtual TState Reduce(StatusUpdated<TState> action, TState state)
            => state with { Status = action.status };

        public async Task HandleAfterEffectAsync(IDispatchContext<TAction> context, CancellationToken cancellationToken)
        {
            var state = Store.GetState<TState>();
            try
            {
                if (state.Status == AsyncOperationStatus.Idle)
                {
                    await DispatchStatusUpdateAsync(AsyncOperationStatus.Loading, cancellationToken);
                    var completedAction = await LoadAsync(context.Action, state);
                    await DispatchStatusUpdateAsync(AsyncOperationStatus.Succeeded, cancellationToken);
                    await Store.DispatchAsync(completedAction, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, state, ex, cancellationToken);
            }
        }

        protected virtual async Task HandleExceptionAsync(IDispatchContext<TAction> context, TState state, Exception ex, CancellationToken cancellationToken)
        {
            await DispatchStatusUpdateAsync(AsyncOperationStatus.Failed, cancellationToken);
            var errorAction = new AsyncError.Occured(context.Action, state, Store.GetState<TState>(), ex);
            await Store.DispatchAsync(errorAction, cancellationToken);
        }

        protected virtual async Task DispatchStatusUpdateAsync(AsyncOperationStatus status, CancellationToken cancellationToken)
        {
            await Store.DispatchAsync(new StatusUpdated<TState>(status), cancellationToken);
        }

        protected abstract Task<TSuccessAction> LoadAsync(TAction action, TState initalState, CancellationToken cancellationToken = default);
    }
}
