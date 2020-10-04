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
    public abstract class AsyncOperation<TAction, TState, TSuccessAction> : IActionAfterEffects<TAction>, IReducer<StatusUpdated, TState>
        where TAction : IAction
        where TState : AsyncState
        where TSuccessAction : IAction
    {
        public AsyncOperation(IStore store)
        {
            Store = store ?? throw new ArgumentNullException(nameof(store));
        }

        protected IStore Store { get; }

        public virtual TState Reduce(StatusUpdated action, TState state) => state with { Status = action.status };

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
            var errorAction = new AsyncErrorOccured(context.Action, state, Store.GetState<TState>(), ex);
            await Store.DispatchAsync(errorAction, cancellationToken);
        }

        protected virtual async Task DispatchStatusUpdateAsync(AsyncOperationStatus status, CancellationToken cancellationToken)
        {
            await Store.DispatchAsync(new StatusUpdated(status), cancellationToken);
        }

        protected abstract Task<TSuccessAction> LoadAsync(TAction action, TState initalState, CancellationToken cancellationToken = default);
    }
    public abstract record AsyncState : StateBase
    {
        public AsyncOperationStatus Status { get; init; }
    }
    public enum AsyncOperationStatus
    {
        Idle,
        Loading,
        Succeeded,
        Failed,
    }
    public record StatusUpdated(AsyncOperationStatus status) : IAction;

    // AsyncErrorOccured
    public record AsyncErrorOccured(IAction Action, AsyncState InitialState, AsyncState ActualState, Exception Exception) : IAction;
    public record AsyncErrorState : StateBase
    {
        public IAction Action { get; init; }
        public AsyncState InitialState { get; init; }
        public AsyncState ActualState { get; init; }
        public Exception Exception { get; init; }

        public bool HasException() => Exception != null;
        public bool HasActualState() => ActualState != null;
        public bool HasInitialState() => InitialState != null;
        public bool HasAction() => Action != null;
    }

    public class InitialAsyncErrorState : IInitialState<AsyncErrorState>
    {
        public AsyncErrorState Value => new();
    }

    public class AsyncErrorOccuredReducer : IReducer<AsyncErrorOccured, AsyncErrorState>
    {
        public AsyncErrorState Reduce(AsyncErrorOccured action, AsyncErrorState initialState) => initialState with {
            Action = action.Action,
            InitialState = action.InitialState,
            ActualState = action.ActualState,
            Exception = action.Exception
        };
    }

    public static class AsyncLogicStartupExtensions
    {
        public static IStatorBuilder AddAsyncOperations(this IStatorBuilder builder)
        {
            builder.AddTypes(new[] {
                typeof(StatusUpdated),
            });
            return builder.AddAsyncErrors();
        }

        public static IStatorBuilder AddAsyncErrors(this IStatorBuilder builder)
        {
            ////var asyncStateType = typeof(AsyncState);
            //var types = new[]
            //{
            //    typeof(ReducerHandler<AsyncErrorState, AsyncErrorOccured>),
            //    typeof(AsyncErrorOccuredReducer),
            //    typeof(InitialAsyncErrorState),
            //    typeof(AsyncErrorState)
            //};
            ////var types = asyncStateType.Assembly.GetTypes().Where(t => t.Namespace == asyncStateType.Namespace);
            //builder.AddTypes(types);
            builder.Services
                .AddSingleton<IActionHandler<AsyncErrorOccured>, ReducerHandler<AsyncErrorState, AsyncErrorOccured>>()
                .AddSingleton<IReducer<AsyncErrorOccured, AsyncErrorState>, AsyncErrorOccuredReducer>()
                .AddSingleton<IInitialState<AsyncErrorState>, InitialAsyncErrorState>()
                .AddSingleton<IState<AsyncErrorState>, Internal.State<AsyncErrorState>>()
            ;
            return builder;
        }
    }
}
