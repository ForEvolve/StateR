using StateR.Updaters;

namespace StateR.AsyncLogic;

public class AsyncError
{
    public record State : StateBase
    {
        public IAction<State>? Action { get; init; }
        public AsyncState? InitialState { get; init; }
        public AsyncState? ActualState { get; init; }
        public Exception? Exception { get; init; }

        public bool HasException() => Exception != null;
        public bool HasActualState() => ActualState != null;
        public bool HasInitialState() => InitialState != null;
        public bool HasAction() => Action != null;
    }
    public class InitialState : IInitialState<State>
    {
        public State Value => new();
    }

    public record Occured(IAction<State> Action, AsyncState InitialState, AsyncState ActualState, Exception Exception) : IAction<State>;

    public class Updaters : IUpdater<Occured, State>
    {
        public State Update(Occured action, State initialState) => initialState with
        {
            Action = action.Action,
            InitialState = action.InitialState,
            ActualState = action.ActualState,
            Exception = action.Exception
        };
    }
}
