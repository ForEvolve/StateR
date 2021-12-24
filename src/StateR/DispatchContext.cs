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
        => _cancellationTokenSource.Cancel(true);
}
