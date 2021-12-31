namespace StateR;

public class DispatchContext<TAction> : IDispatchContext<TAction>
    where TAction : IAction
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
        => throw new DispatchCancelledException(Action);
        //=> _cancellationTokenSource.Cancel(true);
}

public class DispatchCancelledException : Exception
{
    public DispatchCancelledException(IAction action)
        : base($"The dispatch operation '{action.GetType().FullName}' has been cancelled.") { }
}