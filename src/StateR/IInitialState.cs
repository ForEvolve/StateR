namespace StateR;

public interface IInitialState<TState>
    where TState : StateBase
{
    TState Value { get; }
}
