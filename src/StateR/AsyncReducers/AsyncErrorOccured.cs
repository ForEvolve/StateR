using System;

namespace StateR
{
    public record AsyncErrorOccured(IAction Action, AsyncState InitialState, AsyncState ActualState, Exception Exception) : IAction;
    public record AsyncErrorState(IAction Action, AsyncState InitialState, AsyncState ActualState, Exception Exception) : StateBase
    {
        public bool HasError()
        {
            return Exception != null;
        }
    }

    public class InitialAsyncErrorState : IInitialState<AsyncErrorState>
    {
        public AsyncErrorState Value => new AsyncErrorState(default, default, default, default);
    }

    public class AsyncErrorOccuredReducer : IReducer<AsyncErrorOccured, AsyncErrorState>
    {
        public AsyncErrorState Reduce(AsyncErrorOccured action, AsyncErrorState initialState)
            => initialState with { Action = action.Action, InitialState = action.InitialState, ActualState = action.ActualState, Exception = action.Exception };
    }
}