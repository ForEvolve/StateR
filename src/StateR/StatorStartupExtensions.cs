using StateR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

        //internal static readonly Type iRequestHandlerType = typeof(IRequestHandler<,>);
        //internal static readonly Type unitType = typeof(Unit);

        public static IStatorBuilder AddStateR(this IServiceCollection services)
        {
            services.AddSingleton<IStore, Store>();
            services.AddSingleton<IDispatcher, Dispatcher>();
            return new StatorBuilder(services);
        }

        public static IStatorBuilder AddStateR(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            var builder = services.AddStateR();
            var allTypes = assembliesToScan.Concat(new[] { typeof(StatorStartupExtensions).Assembly }).SelectMany(a => a.GetTypes());
            return builder.AddTypes(allTypes);
        }

        //public static IStatorBuilder AddInternalTypes(this IStatorBuilder builder)
        //{
        //    var thisAssembly = typeof(StatorStartupExtensions).Assembly;
        //    var allTypes = thisAssembly.GetTypes();
        //    return builder.AddTypes(allTypes);
        //}

        public static IStatorBuilder Output(this IStatorBuilder builder)
        {
            foreach (var state in builder.States)
            {
                Console.WriteLine($"state: {state.FullName}");
            }
            foreach (var action in builder.Actions)
            {
                Console.WriteLine($"action: {action.FullName}");
            }
            foreach (var interceptor in builder.Interceptors)
            {
                Console.WriteLine($"interceptor: {interceptor.FullName}");
            }
            foreach (var actionHandler in builder.ActionHandlers)
            {
                Console.WriteLine($"actionHandler: {actionHandler.FullName}");
            }
            foreach (var afterEffects in builder.AfterEffects)
            {
                Console.WriteLine($"afterEffects: {afterEffects.FullName}");
            }
            foreach (var reducer in builder.Reducers)
            {
                Console.WriteLine($"reducer: {reducer.FullName}");
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

        public static IStatorBuilder AddAfterEffects(this IStatorBuilder builder)
        {
            var iAfterEffectType = typeof(IAfterEffects<>);
            builder.AfterEffects.ForEach(afterEffectType => {
                afterEffectType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iAfterEffectType)
                    .ToList().ForEach(iAfterEffectInterfaceType =>
                    {
                        Console.WriteLine($"[AddAfterEffects] afterEffectType: {afterEffectType} | iAfterEffectInterfaceType: {iAfterEffectInterfaceType}");
                        var actionType = iAfterEffectInterfaceType.GenericTypeArguments[0];
                        var serviceType = iAfterEffectType.MakeGenericType(actionType);
                        Console.WriteLine($"[AddAfterEffects] actionType: {actionType} | serviceType: {serviceType}");

                        // Equivalent to: AddSingleton<IAfterEffects<TAction>, SomeAfterEffectsImplementation<TAction>>();
                        builder.Services.AddSingleton(serviceType, afterEffectType);
                    });
            });
            return builder;
        }
        public static IStatorBuilder AddReducers(this IStatorBuilder builder)
        {
            var iReducerType = typeof(IReducer<,>);
            var reducerHandler = typeof(ReducerHandler<,>);
            var handlerType = typeof(IActionHandler<>);
            return SharedAddReducers(builder, iReducerType, reducerHandler, handlerType);
        }

        public static IStatorBuilder AddAsyncReducers(this IStatorBuilder builder)
        {
            builder.AddState<AsyncErrorState, InitialAsyncErrorState>();

            // Default error handlers
            // TODO: parametrize that so consumers can customize it
            //builder.Services
            //    .AddSingleton<IActionHandler<AsyncErrorOccured>, ReducerHandler<AsyncErrorState, AsyncErrorOccured>>()
            //    .AddSingleton<IReducer<AsyncErrorOccured, AsyncErrorState>, AsyncErrorOccuredReducer>()
            //;
            //builder.AddTypes(new[] {
            //    typeof(ReducerHandler<AsyncErrorState, AsyncErrorOccured>),
            //    typeof(AsyncErrorOccuredReducer),
            //    typeof(AsyncErrorOccured)
            //});

            // Scan for IAsyncReducer
            var iReducerType = typeof(IAsyncReducer<,>);
            var reducerHandler = typeof(AsyncReducerHandler<,>);
            var handlerType = typeof(IAfterEffects<>);
            return SharedAddReducers(builder, iReducerType, reducerHandler, handlerType);
            //throw new NotImplementedException();
        }

        private static IStatorBuilder SharedAddReducers(IStatorBuilder builder, Type iReducerType, Type reducerHandler, Type handlerType)
        {
            //var reducers = builder.All
            //    .Where(type => !type.IsAbstract && type
            //        .GetTypeInfo()
            //        .GetInterfaces()
            //        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType)
            //    ).ToList();
            var reducers = builder.Reducers;
            reducers.ForEach(reducerType => reducerType
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType)
                    .ToList().ForEach(reducerInterfaceType =>
                    {
                        // Equivalent to: AddSingleton<IActionHandler<TAction>, [Async]ReducerHandler<TState, TAction>>
                        var actionType = reducerInterfaceType.GenericTypeArguments[0];
                        var stateType = reducerInterfaceType.GenericTypeArguments[1];
                        var iActionHandlerServiceType = handlerType.MakeGenericType(actionType);
                        var reducerHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                        builder.Services.AddSingleton(iActionHandlerServiceType, reducerHandlerImplementationType);

                        // Equivalent to: AddSingleton<I[Async]Reducer<TState, TAction>, Reducer>();
                        builder.Services.AddSingleton(reducerInterfaceType, reducerType);
                    })
                );
            return builder;
        }

        //public static IStatorBuilder AddLoggingDispatcher(this IStatorBuilder builder)
        //{
        //    builder.Services.AddSingleton<IDispatcher, LoggingDispatcher>();
        //    return builder;
        //}

        private class StatorBuilder : IStatorBuilder
        {
            public StatorBuilder(IServiceCollection services)
            {
                Services = services ?? throw new ArgumentNullException(nameof(services));
            }

            public static IEnumerable<Type> FindStates(IEnumerable<Type> types)
            {
                var states = types.Where(type => !type.IsAbstract && type.IsSubclassOf(baseStateType));
                return states;
            }
            public static IEnumerable<Type> FindActions(IEnumerable<Type> types)
            {
                var actions = types.Where(type => !type.IsAbstract && type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i == iActionType)
                );
                return actions;
            }
            public IEnumerable<Type> FindInterceptors(IEnumerable<Type> types)
            {
                var iActionInterceptor = typeof(IActionInterceptor<>);
                return types.Where(type => !type.IsAbstract && type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iActionInterceptor)
                );
            }
            public IEnumerable<Type> FindActionHandlers(IEnumerable<Type> types)
            {
                var iActionHandler = typeof(IActionHandler<>);
                return types.Where(type => !type.IsAbstract && type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iActionHandler)
                );
            }
            public IEnumerable<Type> FindAfterEffects(IEnumerable<Type> types)
            {
                var iAfterEffects = typeof(IAfterEffects<>);
                return types.Where(type => !type.IsAbstract && type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iAfterEffects)
                );
            }
            public IEnumerable<Type> FindReducers(IEnumerable<Type> types)
            {
                var iReducerType = typeof(IReducer<,>);
                return types.Where(type => !type.IsAbstract && type
                    .GetTypeInfo()
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType)
                );
            }

            public IStatorBuilder AddTypes(IEnumerable<Type> types)
            {
                All.AddRange(types);
                States.AddRange(FindStates(types));
                Actions.AddRange(FindActions(types));
                Interceptors.AddRange(FindInterceptors(types));
                ActionHandlers.AddRange(FindActionHandlers(types));
                AfterEffects.AddRange(FindAfterEffects(types));
                Reducers.AddRange(FindReducers(types));
                return this;
            }

            public IServiceCollection Services { get; }
            public List<Type> Actions { get; } = new List<Type>();
            public List<Type> States { get; } = new List<Type>();
            public List<Type> Interceptors { get; } = new List<Type>();
            public List<Type> ActionHandlers { get; } = new List<Type>();
            public List<Type> AfterEffects { get; } = new List<Type>();
            public List<Type> Reducers { get; } = new List<Type>();
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