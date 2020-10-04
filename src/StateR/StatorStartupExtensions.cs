﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.AfterEffects;
using StateR.Interceptors;
using StateR.Internal;
using StateR.Reducers;
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
            services.TryAddSingleton<IReducersManager, ReducersManager>();
            services.TryAddSingleton<IAfterEffectsManager, AfterEffectsManager>();
            services.TryAddSingleton<IDispatchContextFactory, DispatchContextFactory>();
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

                // Equivalent to: AddSingleton<IInterceptorsMiddleware, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IInterceptorsMiddleware)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IAfterEffectsMiddleware, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterEffectsMiddleware)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IActionHandlerMiddleware, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IActionHandlerMiddleware)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IReducersMiddleware, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IReducersMiddleware)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IActionInterceptor<TState>, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IActionInterceptor<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()

                // Equivalent to: AddSingleton<IActionAfterEffects<TState>, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IActionAfterEffects<>)))
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
                    // Equivalent to: TryAddSingleton<IActionHandler<TAction>, ReducerHandler<TState, TAction>>
                    var actionType = @interface.GenericTypeArguments[0];
                    var stateType = @interface.GenericTypeArguments[1];
                    var iActionHandlerServiceType = handlerType.MakeGenericType(actionType);
                    var reducerHandlerImplementationType = reducerHandler.MakeGenericType(stateType, actionType);
                    builder.Services.TryAddSingleton(iActionHandlerServiceType, reducerHandlerImplementationType);

                    // Equivalent to: AddSingleton<IReducer<TState, TAction>, Reducer>();
                    builder.Services.AddSingleton(@interface, reducer);

                    Console.WriteLine($"- TryAddSingleton<{iActionHandlerServiceType.GetStatorName()}, {reducerHandlerImplementationType.GetStatorName()}>()");
                    Console.WriteLine($"- AddSingleton<{@interface.GetStatorName()}, {reducer.GetStatorName()}>()");
                }
            }
            return builder.Services;
        }
    }
}