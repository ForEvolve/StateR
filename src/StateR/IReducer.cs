using System;

namespace StateR
{
    public interface IReducer<TState, TAction>
        where TState : StateBase
        where TAction : IAction
    {
        TState Reduce(TAction action, TState initialState);
    }
}