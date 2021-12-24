using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StateR.ActionHandlers;
using StateR.ActionHandlers.Hooks;
using StateR.AfterEffects;
using StateR.AfterEffects.Hooks;
using StateR.Interceptors;
using StateR.Interceptors.Hooks;
using StateR.Internal;
using StateR.Updater;
using StateR.Updater.Hooks;
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
            services.TryAddSingleton<IUpdateHooksCollection, UpdateHooksCollection>();

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

                // Equivalent to: AddSingleton<IBeforeUpdaterHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IBeforeUpdateHook)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
                // Equivalent to: AddSingleton<IAfterUpdaterHook, Implementation>();
                .AddClasses(classes => classes.AssignableTo(typeof(IAfterUpdateHook)))
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

            // Register Updaters and their respective IActionHandler
            var iUpdaterType = typeof(IUpdater<,>);
            var updaterHandler = typeof(UpdaterHandler<,>);
            var handlerType = typeof(IActionHandler<>);
            foreach (var updater in builder.Updaters)
            {
                Console.WriteLine($"updater: {updater.FullName}");
                var interfaces = updater.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == iUpdaterType);
                foreach (var @interface in interfaces)
                {
                    // Equivalent to: AddSingleton<IActionHandler<TAction>, UpdaterHandler<TState, TAction>>
                    var actionType = @interface.GenericTypeArguments[0];
                    var stateType = @interface.GenericTypeArguments[1];
                    var iActionHandlerServiceType = handlerType.MakeGenericType(actionType);
                    var updaterHandlerImplementationType = updaterHandler.MakeGenericType(stateType, actionType);
                    builder.Services.AddSingleton(iActionHandlerServiceType, updaterHandlerImplementationType);

                    // Equivalent to: AddSingleton<IUpdater<TState, TAction>, Updater>();
                    builder.Services.AddSingleton(@interface, updater);

                    Console.WriteLine($"- AddSingleton<{iActionHandlerServiceType.GetStatorName()}, {updaterHandlerImplementationType.GetStatorName()}>()");
                    Console.WriteLine($"- AddSingleton<{@interface.GetStatorName()}, {updater.GetStatorName()}>()");
                }
            }
            return builder.Services;
        }
    }
}
