
using Microsoft.Extensions.DependencyInjection;
using StateR.Internal;
using System.Reflection;

namespace StateR.Blazor.Persistance;

public static class PersistenceStartupExtensions
{
    public static IStatorBuilder AddPersistence(this IStatorBuilder builder)
    {
        foreach (var state in builder.States)
        {
            var persistAttribute = state.GetCustomAttribute<PersistAttribute>();
            if (persistAttribute == null)
            {
                continue;
            }

            Console.WriteLine($"Persistence: {state.GetStatorName()}");
            Console.WriteLine($"- Decorate<IInitialState<{state.GetStatorName()}>, WebStorageInitialStateDecorator<{state.GetStatorName()}>>()");

            // Equivalent to: Decorate<IInitialState<TState>, WebStorageInitialStateDecorator<TState>>();
            var initialStateType = typeof(IInitialState<>).MakeGenericType(state);
            var decoratedInitialStateType = typeof(WebStorageInitialStateDecorator<>).MakeGenericType(state);
            builder.Services.Decorate(initialStateType, decoratedInitialStateType);

            Console.WriteLine($"- Decorate<IState<{state.GetStatorName()}>, WebStorageStateDecorator<{state.GetStatorName()}>>()");

            // Equivalent to: Decorate<IState<TState>, WebStorageStateDecorator<TState>>();
            var stateType = typeof(IState<>).MakeGenericType(state);
            var stateSessionDecoratorType = typeof(WebStorageStateDecorator<>).MakeGenericType(state);
            builder.Services.Decorate(stateType, stateSessionDecoratorType);
        }
        return builder;
    }
}