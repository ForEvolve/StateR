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

            var allTypes = assembliesToScan.SelectMany(a => a.GetTypes()).ToList();
            //var foundStates = allTypes.Where(type => type.IsSubclassOf(baseStateType)).ToList();
            return new StatorBuilder(services, allTypes)
                //.AddInitialStates()
                //.AddStates()
                //.AddReducers()
                //.AddAsyncReducers()
            ;
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

        public static IStatorBuilder AddAsyncReducers(this IStatorBuilder builder)
        {
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
                        // Equivalent to: AddSingleton<IRequestHandler<TAction, Unit>, ReducerHandler<TState, TAction>>
                        var actionType = reducerInterfaceType.GenericTypeArguments[0];
                        var stateType = reducerInterfaceType.GenericTypeArguments[1];
                        var requestHandlerServiceType = iRequestHandlerType.MakeGenericType(actionType, unitType);
                        var requestHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                        builder.Services.AddSingleton(requestHandlerServiceType, requestHandlerImplementationType);

                        // Equivalent to: AddSingleton<IReducer<TState, TAction>, Reducer>();
                        builder.Services.AddSingleton(reducerInterfaceType, reducerType);
                    })
                );
            return builder;
        }

        private class StatorBuilder : IStatorBuilder
        {
            public StatorBuilder(IServiceCollection services, IList<Type> all)
            {
                Services = services ?? throw new ArgumentNullException(nameof(services));
                if (all == null) { throw new ArgumentNullException(nameof(all)); }

                All = new ReadOnlyCollection<Type>(all);
                States = GetStates();
                Actions = GetActions();
            }
            private ReadOnlyCollection<Type> GetStates()
            {
                var states = All.Where(type => type.IsSubclassOf(baseStateType)).ToList();
                return new ReadOnlyCollection<Type>(states);
            }
            private ReadOnlyCollection<Type> GetActions()
            {
                var actions = All.Where(type => type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i == iActionType)
                ).ToList();
                return new ReadOnlyCollection<Type>(actions);
            }

            public IServiceCollection Services { get; }
            public ReadOnlyCollection<Type> Actions { get; }
            public ReadOnlyCollection<Type> States { get; }
            public ReadOnlyCollection<Type> All { get; }
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