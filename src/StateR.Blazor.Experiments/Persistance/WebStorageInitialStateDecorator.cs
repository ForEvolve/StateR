using StateR.Blazor.WebStorage;
using StateR.Internal;

namespace StateR.Blazor.Persistance;

public class WebStorageInitialStateDecorator<TState> : IInitialState<TState>
    where TState : StateBase
{
    private readonly IStorage _storage;
    private readonly IInitialState<TState> _next;
    private readonly string _key;

    public WebStorageInitialStateDecorator(IStorage storage, IInitialState<TState> next)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _key = typeof(TState).GetStatorName();
    }

    public TState Value
    {
        get
        {
            var item = _storage.GetItem<TState>(_key);
            if (item == null)
            {
                Console.WriteLine($"[InitialSessionStateDecorator][{_key}] Not item found in storage.");
                return _next.Value;
            }
            Console.WriteLine($"[InitialSessionStateDecorator][{_key}] Item found: {item}");
            return item;
        }
    }
}
