using StateR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Scrutor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StatorStartupExtensions
    {
        public static IStatorBuilder AddStateR(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            // Register the store
            services.AddSingleton<IStore, Store>();

            // Register initial states
            services.Scan(s => s
                .FromAssemblies(assembliesToScan)
                .AddClasses(classes => classes.AssignableTo(typeof(IInitialState<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

            // Register states
            var iStateType = typeof(IState<>);
            var stateType = typeof(State<>);
            var baseStateType = typeof(StateBase);
            var iBrowsableStateType = typeof(IBrowsableState<>);
            var browsableStateDecoratorType = typeof(BrowsableStateDecorator<>);
            var allTypes = assembliesToScan.SelectMany(a => a.GetTypes());
            allTypes.Where(type => type.IsSubclassOf(baseStateType))
                .ToList()
                .ForEach(type =>
                {
                    // Equivalent to: AddSingleton<IState<TState>, State<TState>>();
                    var stateServiceType = iStateType.MakeGenericType(type);
                    var stateImplementationType = stateType.MakeGenericType(type);
                    services.AddSingleton(stateServiceType, stateImplementationType);

                    // Equivalent to: 
                    // - Decorate<IState<TState>, BrowsableStateDecorator<TState>>();
                    // - AddSingleton<IBrowsableState<TState>>(p => p.GetRequiredService<IState<TState>>());
                    var browsableStateServiceType = iBrowsableStateType.MakeGenericType(type);
                    var browsableStateImplementationType = browsableStateDecoratorType.MakeGenericType(type);
                    services.Decorate(stateServiceType, browsableStateImplementationType);
                    services.AddSingleton(browsableStateServiceType, p => p.GetRequiredService(stateServiceType));
                });

            // Register reducers
            var iReducerType = typeof(IReducer<,>);
            var reducerHandler = typeof(ReducerHandler<,>);
            var iRequestHandlerType = typeof(IRequestHandler<,>);
            var unitType = typeof(Unit);
            allTypes
                .Where(type => type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType)
                ).ToList()
                .ForEach(reducerType => reducerType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType)
                    .ToList().ForEach(reducerInterfaceType =>
                    {
                        // Equivalent to: AddSingleton<IRequestHandler<TAction, Unit>, ReducerHandler<TState, TAction>>
                        var stateType = reducerInterfaceType.GenericTypeArguments[0];
                        var actionType = reducerInterfaceType.GenericTypeArguments[1];
                        var requestHandlerServiceType = iRequestHandlerType.MakeGenericType(actionType, unitType);
                        var requestHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                        services.AddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                        // Equivalent to: AddSingleton<IReducer<TState, TAction>, Reducer>();
                        services.AddSingleton(reducerInterfaceType, reducerType);
                    })
                );

            // Register BrowsableState options
            services
                .Configure<BrowsableStateOptions>(options => { })
                .AddSingleton(ctx => ctx.GetService<IOptionsMonitor<BrowsableStateOptions>>().CurrentValue)
            ;

            // BrowsableStateOptions
            return new StatorBuilder(services);
        }

        private class StatorBuilder : IStatorBuilder
        {
            public StatorBuilder(IServiceCollection services)
            {
                Services = services ?? throw new ArgumentNullException(nameof(services));
            }
            public IServiceCollection Services { get; }
        }

        private class InitialState<TState> : IInitialState<TState>
            where TState : StateBase
        {
            public InitialState(TState value)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value));
            }

            public TState Value { get; }
        }

        private class State<TState> : IState<TState>
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

            public void Transform(Func<TState, TState> stateTransform)
            {
                var newState = stateTransform(Current);
                Set(newState);
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

        private class BrowsableStateDecorator<TState> : IBrowsableState<TState>, IState<TState>, IDisposable
            where TState : StateBase
        {
            private readonly List<StateHistoryEntry<TState>> _states = new();
            private readonly BrowsableStateOptions _options;

            private int _cursorIndex;

            public BrowsableStateDecorator(IState<TState> state, BrowsableStateOptions options)
            {
                _state = state ?? throw new ArgumentNullException(nameof(state));
                _options = options ?? throw new ArgumentNullException(nameof(options));
                _state.Subscribe(StateChanged);
                PushCurrentState();
            }

            private void StateChanged()
            {
                if (Current == Cursor?.State)
                {
                    return;
                }
                PushCurrentState();
            }

            private void PushCurrentState() => PushState(Current);
            private void PushState(TState state)
            {
                if (state == Cursor?.State)
                {
                    return;
                }

                _states.Add(new StateHistoryEntry<TState>(state, DateTime.UtcNow));
                ResetCursorPosition();

                if (_options.HasAMaxHistoryLength() && _states.Count >= _options.MaxHistoryLength)
                {
                    _states.RemoveAt(0);
                    ResetCursorPosition();
                }

                void ResetCursorPosition()
                {
                    _cursorIndex = _states.Count - 1;
                }
            }

            private StateHistoryEntry<TState>[] _history;
            private int _lastStateCount = 0;
            public IReadOnlyCollection<StateHistoryEntry<TState>> History
            {
                get
                {
                    if (_history is null || _states.Count != _lastStateCount)
                    {
                        _history = _states.ToArray().Reverse().ToArray();
                        _lastStateCount = _states.Count;
                    }
                    return _history;
                }
            }

            public StateHistoryEntry<TState> Cursor
                => _states.Count > _cursorIndex ? _states[_cursorIndex] : default;

            public bool Redo()
            {
                if (_cursorIndex + 1 >= _states.Count)
                {
                    return false;
                }
                _cursorIndex++;
                Set(Cursor.State);
                Notify();
                return true;
            }

            public bool Undo()
            {
                if (_cursorIndex - 1 < 0)
                {
                    return false;
                }
                _cursorIndex--;
                Set(Cursor.State);
                Notify();
                return true;
            }

            #region IState<TState> Facade

            private readonly IState<TState> _state;

            public TState Current => _state.Current;

            public void Set(TState state)
            {
                _state.Set(state);
            }

            public void Subscribe(Action stateHasChangedDelegate)
            {
                _state.Subscribe(stateHasChangedDelegate);
            }

            public void Transform(Func<TState, TState> stateTransform)
            {
                _state.Transform(stateTransform);
            }

            public void Unsubscribe(Action stateHasChangedDelegate)
            {
                _state.Unsubscribe(stateHasChangedDelegate);
            }

            public void Notify()
            {
                _state.Notify();
            }

            #endregion

            #region IDisposable

            private bool disposedValue;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        Unsubscribe(StateChanged);
                    }
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            #endregion
        }
    }
}