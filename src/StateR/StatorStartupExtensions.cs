﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.ActionHandlers;
using StateR.ActionHandlers.Hooks;
using StateR.AfterEffects;
using StateR.AfterEffects.Hooks;
using StateR.Interceptors;
using StateR.Interceptors.Hooks;
using StateR.Internal;
using StateR.Reducers;
using StateR.Reducers.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StateR
{
    public static class StatorStartupExtensions
    {
        public static IStatorBuilder AddStateR(this IServiceCollection services)
        {
            services.TryAddSingleton<IStore, Store>();
            services.TryAddSingleton<IDispatcher, Dispatcher>();

            services.TryAddSingleton<IInterceptorsManager, InterceptorsManager>();
            services.TryAddSingleton<IActionHandlersManager, ActionHandlersManager>();
            services.TryAddSingleton<IAfterEffectsManager, AfterEffectsManager>();
            services.TryAddSingleton<IDispatchContextFactory, DispatchContextFactory>();

            services.TryAddSingleton<IAfterEffectHooksCollection, AfterEffectHooksCollection>();
            services.TryAddSingleton<IInterceptorsHooksCollection, InterceptorsHooksCollection>();
            services.TryAddSingleton<IActionHandlerHooksCollection, ActionHandlerHooksCollection>();
            services.TryAddSingleton<IReducerHooksCollection, ReducerHooksCollection>();

            return new StatorBuilder(services);
        }

        public static IStatorBuilder AddStateR(this IServiceCollection services, params Assembly[] assembliesToScan)
        {
            var builder = services.AddStateR();
            var allTypes = assembliesToScan
                .SelectMany(a => a.GetTypes());
            return builder.AddTypes(allTypes);
        }

        public static IServiceCollection Apply(this IStatorBuilder builder)
        {
            // Extract types
            builder.ScanTypes();

            // Scan
            builder.Services.Scan(s => s
                .AddTypes(builder.All)

                // Equivalent to: AddSingleton<IInitialState<TState>, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IInitialState<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IBeforeInterceptorHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IBeforeInterceptorHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                // Equivalent to: AddSingleton<IAfterInterceptorHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterInterceptorHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IBeforeAfterEffectHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IBeforeAfterEffectHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                // Equivalent to: AddSingleton<IAfterAfterEffectHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterAfterEffectHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IBeforeActionHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IBeforeActionHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                // Equivalent to: AddSingleton<IAfterActionHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterActionHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IBeforeReducerHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IBeforeReducerHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                // Equivalent to: AddSingleton<IAfterReducerHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterReducerHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IInterceptor<TState>, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IInterceptor<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IActionAfterEffects<TState>, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterEffects<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

            // Register States
            foreach (var state in builder.States)
            {
                Console.WriteLine($"state: {state.FullName}");

                // Equivalent to: AddSingleton<IState<TState>, State<TState>>();
                var stateServiceType = typeof(IState<>).MakeGenericType(state);
                var stateImplementationType = typeof(State<>).MakeGenericType(state);
                builder.Services.AddSingleton(stateServiceType, stateImplementationType);
            }

            // Register Reducers and their respective IActionHandler
            var iReducerType = typeof(IReducer<,>);
            var reducerHandler = typeof(ReducerHandler<,>);
            var handlerType = typeof(IActionHandler<>);
            foreach (var reducer in builder.Reducers)
            {
                Console.WriteLine($"reducer: {reducer.FullName}");
                var interfaces = reducer.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iReducerType);
                foreach (var @interface in interfaces)
                {
                    // Equivalent to: AddSingleton<IActionHandler<TAction>, ReducerHandler<TState, TAction>>
                    var actionType = @interface.GenericTypeArguments[0];
                    var stateType = @interface.GenericTypeArguments[1];
                    var iActionHandlerServiceType = handlerType.MakeGenericType(actionType);
                    var reducerHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                    builder.Services.AddSingleton(iActionHandlerServiceType, reducerHandlerImplementationType);

                    // Equivalent to: AddSingleton<IReducer<TState, TAction>, Reducer>();
                    builder.Services.AddSingleton(@interface, reducer);

                    Console.WriteLine($"- AddSingleton<{iActionHandlerServiceType.GetStatorName()}, {reducerHandlerImplementationType.GetStatorName()}>()");
                    Console.WriteLine($"- AddSingleton<{@interface.GetStatorName()}, {reducer.GetStatorName()}>()");
                }
            }
            return builder.Services;
        }
    }
}
