using System;

namespace StateR
{
    public record AsyncErrorState<TLoadAction, TState, TResponse>(TLoadAction action, TState State, Exception Exception)
        where TLoadAction : IAsyncAction<TResponse>
        where TState : AsyncState;
}