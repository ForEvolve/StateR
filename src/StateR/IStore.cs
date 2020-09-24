using System;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public interface IStore
    {
        TState GetState<TState>() where TState : StateBase;
        void SetState<TState>(Func<TState, TState> stateTransform) where TState : StateBase;
        void Subscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase;
        void Unsubscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase;
        Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction;
    }
}
