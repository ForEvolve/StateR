﻿using StateR;
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
using System.Collections.ObjectModel;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StatorStartupExtensions
    {
        internal static readonly Type baseStateType = typeof(StateBase);
        internal static readonly Type iStateType = typeof(IState<>);
        internal static readonly Type stateType = typeof(State<>);
        internal static readonly Type iActionType = typeof(IAction);

        internal static readonly Type iRequestHandlerType = typeof(IRequestHandler<,>);
        internal static readonly Type unitType = typeof(Unit);

        public static IStatorBuilder AddStateR(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            services.AddMediatR(assembliesToScan);
            services.AddSingleton<IStore, Store>();

            var allTypes = assembliesToScan.SelectMany(a => a.GetTypes());
            return new StatorBuilder(services, allTypes);
        }

        //public static IStatorBuilder AddInternalTypes(this IStatorBuilder builder)
        //{
        //    var thisAssembly = typeof(StatorStartupExtensions).Assembly;
        //    var allTypes = thisAssembly.GetTypes();
        //    return builder.AddTypes(allTypes);
        //}

        public static IStatorBuilder OutputStates(this IStatorBuilder builder)
        {
            foreach (var state in builder.States)
            {
                Console.WriteLine($"state: {state.FullName}");
            }
            return builder;
        }

        public static IStatorBuilder OutputActions(this IStatorBuilder builder)
        {
            foreach (var action in builder.Actions)
            {
                Console.WriteLine($"action: {action.FullName}");
            }
            return builder;
        }

        public static IStatorBuilder AddInitialStates(this IStatorBuilder builder)
        {
            var iInitialStateType = typeof(IInitialState<>);
            builder.Services.Scan(s => s
                .AddTypes(builder.All)
                .AddClasses(classes => classes.AssignableTo(iInitialStateType))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );
            return builder;
        }

        public static IStatorBuilder AddStates(this IStatorBuilder builder)
        {
            foreach (var type in builder.States)
            {
                // Equivalent to: AddSingleton<IState<TState>, State<TState>>();
                var stateServiceType = iStateType.MakeGenericType(type);
                var stateImplementationType = stateType.MakeGenericType(type);
                builder.Services.AddSingleton(stateServiceType, stateImplementationType);
            }
            return builder;
        }


        public static IStatorBuilder AddReducers(this IStatorBuilder builder)
        {

            var iReducerType = typeof(IReducer<,>);
            var reducerHandler = typeof(ReducerHandler<,>);
            return SharedAddReducers(builder, iReducerType, reducerHandler);
        }

        public static IStatorBuilder AddState<TState, TInitialState>(this IStatorBuilder builder)
            where TState : StateBase
            where TInitialState : class, IInitialState<TState>
        {
            builder.Services
                .AddSingleton<IInitialState<TState>, TInitialState>()
                .AddSingleton<IState<TState>, State<TState>>()
            ;
            builder.AddTypes(new[] {
                typeof(TState),
                typeof(TInitialState),
            });
            return builder;
        }

        public static IStatorBuilder AddAsyncReducers(this IStatorBuilder builder)
        {
            builder.AddState<AsyncErrorState, InitialAsyncErrorState>();

            // Default error handlers
            // TODO: parametrize that so consumers can customize it
            builder.Services
                .AddSingleton<IRequestHandler<AsyncErrorOccured, Unit>, ReducerHandler<AsyncErrorState, AsyncErrorOccured>>()
                .AddSingleton<IReducer<AsyncErrorOccured, AsyncErrorState>, AsyncErrorOccuredReducer>()
            ;
            builder.AddTypes(new[] {
                typeof(ReducerHandler<AsyncErrorState, AsyncErrorOccured>),
                typeof(AsyncErrorOccuredReducer),
                typeof(AsyncErrorOccured)
            });

            // Scan for IAsyncReducer
            var iReducerType = typeof(IAsyncReducer<,>);
            var reducerHandler = typeof(AsyncReducerHandler<,>);
            return SharedAddReducers(builder, iReducerType, reducerHandler);
        }

        private static IStatorBuilder SharedAddReducers(IStatorBuilder builder, Type iReducerType, Type reducerHandler)
        {
            builder.All
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
                        // Equivalent to: AddSingleton<IRequestHandler<TAction, Unit>, [Async]ReducerHandler<TState, TAction>>
                        var actionType = reducerInterfaceType.GenericTypeArguments[0];
                        var stateType = reducerInterfaceType.GenericTypeArguments[1];
                        var requestHandlerServiceType = iRequestHandlerType.MakeGenericType(actionType, unitType);
                        var requestHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                        builder.Services.AddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                        // Equivalent to: AddSingleton<I[Async]Reducer<TState, TAction>, Reducer>();
                        builder.Services.AddSingleton(reducerInterfaceType, reducerType);
                    })
                );
            return builder;
        }

        private class StatorBuilder : IStatorBuilder
        {
            public StatorBuilder(IServiceCollection services, IEnumerable<Type> all)
            {
                Services = services ?? throw new ArgumentNullException(nameof(services));
                if (all == null) { throw new ArgumentNullException(nameof(all)); }

                AddTypes(all);
            }

            public static IEnumerable<Type> GetStates(IEnumerable<Type> types)
            {
                var states = types.Where(type => type.IsSubclassOf(baseStateType));
                return states;
            }
            public static IEnumerable<Type> GetActions(IEnumerable<Type> types)
            {
                var actions = types.Where(type => type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i == iActionType)
                );
                return actions;
            }

            public IStatorBuilder AddTypes(IEnumerable<Type> types)
            {
                All.AddRange(types);
                States.AddRange(GetStates(types));
                Actions.AddRange(GetActions(types));
                return this;
            }

            public IServiceCollection Services { get; }
            public List<Type> Actions { get; } = new List<Type>();
            public List<Type> States { get; } = new List<Type>();
            public List<Type> All { get; } = new List<Type>();
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
    }
}