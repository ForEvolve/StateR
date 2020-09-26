using System;

namespace StateR
{
    public record AsyncErrorState<TLoadAction, TState>(TLoadAction action, TState State, Exception Exception)
        where TLoadAction : IAction
        where TState : AsyncState;
}