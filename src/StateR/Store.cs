using Microsoft.Extensions.DependencyInjection;

namespace StateR;

public class Store : IStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDispatcher _dispatcher;

    public Store(IServiceProvider serviceProvider, IDispatcher dispatcher)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public Task DispatchAsync<TAction, TState>(TAction action, CancellationToken cancellationToken = default)
        where TAction : IAction<TState>
        where TState : StateBase
    {
        return _dispatcher.DispatchAsync<TAction, TState>(action, cancellationToken);
    }

    public Task DispatchAsync(object action, CancellationToken cancellationToken)
    {
        return _dispatcher.DispatchAsync(action, cancellationToken);
    }

    public TState GetState<TState>() where TState : StateBase
    {
        var state = _serviceProvider.GetRequiredService<IState<TState>>();
        return state.Current;
    }

    public void SetState<TState>(Func<TState, TState> stateTransform) where TState : StateBase
    {
        var state = _serviceProvider.GetRequiredService<IState<TState>>();
        state.Set(stateTransform(state.Current));
        state.Notify();
    }

    public void Subscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase
    {
        var state = _serviceProvider.GetRequiredService<IState<TState>>();
        state.Subscribe(stateHasChangedDelegate);
    }

    public void Unsubscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase
    {
        var state = _serviceProvider.GetRequiredService<IState<TState>>();
        state.Unsubscribe(stateHasChangedDelegate);
    }
}
