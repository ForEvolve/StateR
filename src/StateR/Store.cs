using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace StateR
{
    public class Store : IStore
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        public Store(IServiceProvider serviceProvider, IMediator mediator)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task DispatchAsync<TAction>(TAction action, CancellationToken cancellationToken = default) where TAction : IAction
        {
            await _mediator.Send(action, cancellationToken);
            await _mediator.Publish(action, cancellationToken);
        }

        public TState GetState<TState>() where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            return state.Current;
        }

        public void SetState<TState>(Func<TState, TState> stateTransform) where TState : StateBase
        {
            var state = _serviceProvider.GetRequiredService<IState<TState>>();
            state.Transform(stateTransform);
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
