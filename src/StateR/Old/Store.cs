using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace StateR
{
    public class Store : IStore
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDispatcher _dispatcher;
        public Store(IServiceProvider serviceProvider, IDispatcher dispatcher)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction
        {
            return _dispatcher.DispatchAsync(action, cancellationToken);
        }

        public TState GetState<TState>() where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            return state.Current;
        }

        public void SetState<TState>(Func<TState, TState> stateTransform) where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            state.Set(stateTransform(state.Current));
            state.Notify();
        }

        public void Subscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            state.Subscribe(stateHasChangedDelegate);
        }

        public void Unsubscribe<TState>(Action stateHasChangedDelegate) where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            state.Unsubscribe(stateHasChangedDelegate);
        }
    }
}
