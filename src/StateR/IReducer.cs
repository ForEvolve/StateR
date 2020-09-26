using System;

namespace StateR
{
    public interface IReducer<TAction, TState>
        where TAction : IAction
        where TState : StateBase
    {
        TState Reduce(TAction action, TState initialState);
    }
}