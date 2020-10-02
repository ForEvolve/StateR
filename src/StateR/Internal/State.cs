using System;
using System.Collections.Generic;

namespace StateR.Internal
{
    public class State<TState> : IState<TState>
        where TState : StateBase
    {
        private readonly List<Action> _subscribers = new();
        private readonly object _subscriberLock = new();

        public State(IInitialState<TState> initial)
            => Set(initial.Value);

        public TState Current { get; private set; }

        public void Set(TState state)
        {
            if (Current == state)
            {
                return;
            }
            Current = state;
        }

        public void Subscribe(Action stateHasChangedDelegate)
        {
            lock (_subscriberLock)
            {
                _subscribers.Add(stateHasChangedDelegate);
            }
        }

        public void Unsubscribe(Action stateHasChangedDelegate)
        {
            lock (_subscriberLock)
            {
                _subscribers.Remove(stateHasChangedDelegate);
            }
        }

        public void Notify()
        {
            lock (_subscriberLock)
            {
                foreach (var subscriber in _subscribers)
                {
                    subscriber();
                }
            }
        }
    }
}