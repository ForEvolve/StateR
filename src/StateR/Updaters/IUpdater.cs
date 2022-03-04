namespace StateR.Updaters;

public interface IUpdater<TAction, TState>
    where TAction : IAction<TState>
    where TState : StateBase
{
    TState Update(TAction action, TState state);
}
