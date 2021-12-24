namespace StateR.Internal;

public class InitialState<TState> : IInitialState<TState>
        where TState : StateBase
{
    public InitialState(TState value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public TState Value { get; }
}
