namespace StateR;

public class DispatchContext<TAction, TState> : IDispatchContext<TAction, TState>
    where TAction : IAction<TState>
    where TState : StateBase
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    public DispatchContext(TAction action, IDispatcher dispatcher, CancellationTokenSource cancellationTokenSource)
    {
        Action = action;
        Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
    }

    public IDispatcher Dispatcher { get; }
    public TAction Action { get; }
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public void Cancel()
        => throw new DispatchCancelledException(Action.GetType());
}

public class DispatchCancelledException : Exception
{
    public DispatchCancelledException(Type actionType)
        : base($"The dispatch operation '{actionType.FullName}' has been cancelled.") { }
}