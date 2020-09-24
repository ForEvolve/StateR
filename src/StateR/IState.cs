using System;

namespace StateR
{
    public interface IState<TState> : ISubscribable
        where TState : StateBase
    {
        TState Current { get; }
        void Set(TState state);
        void Transform(Func<TState, TState> stateTransform);
    }
}
