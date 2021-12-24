namespace StateR;

public interface IStore : IDispatcher
{
    TState GetState<TState>() where TState : StateBase;
    void SetState<TState>(Func<TState, TState> stateTransform) where TState : StateBase;
    void Subscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase;
    void Unsubscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase;
}
