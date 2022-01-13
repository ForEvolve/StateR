using ForEvolve.Blazor.WebStorage;
using StateR.Internal;

namespace StateR.Blazor.Persistance;

public class WebStorageStateDecorator<TState> : IState<TState>
    where TState : StateBase
{
    private readonly IStorage _storage;
    private readonly IState<TState> _next;
    private readonly string _key;

    public WebStorageStateDecorator(IStorage storage, IState<TState> next)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _key = typeof(TState).GetStatorName();
    }

    public void Set(TState state)
    {
        _next.Set(state);
        Console.WriteLine($"[SessionStateDecorator][{_key}] Set: {state}.");
        _storage.SetItem(_key, state);
    }

    public TState Current => _next.Current;
    public void Notify() => _next.Notify();
    public void Subscribe(Action stateHasChangedDelegate)
        => _next.Subscribe(stateHasChangedDelegate);
    public void Unsubscribe(Action stateHasChangedDelegate)
        => _next.Unsubscribe(stateHasChangedDelegate);
}
