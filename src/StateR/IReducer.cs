using System;
namespace StateR
{
    public interface IReducer<TState, TAction>
        where TState : StateBase
        where TAction : IAction
    {
        TState Reduce(TState state, TAction action);
    }
}