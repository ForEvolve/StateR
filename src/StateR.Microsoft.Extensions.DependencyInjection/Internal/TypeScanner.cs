using StateR.Pipeline;
using StateR.Updaters;
using System.Reflection;

namespace StateR.Internal;

public static class TypeScannerExtensions
{
    public static IEnumerable<Type> FindStates(this IEnumerable<Type> types)
    {
        var states = types
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(StateBase)));
        return states;
    }

    public static IEnumerable<Type> FindInitialStates(this IEnumerable<Type> types)
    {
        var initialStates = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i == typeof(IInitialState<>))
        );
        return initialStates;
    }

    public static IEnumerable<Type> FindActions(this IEnumerable<Type> types)
    {
        var actions = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i == typeof(IAction<>))
        );
        return actions;
    }

    public static IEnumerable<Type> FindUpdaters(this IEnumerable<Type> types)
    {
        var updaters = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUpdater<,>))
        );
        return updaters;
    }

    public static IEnumerable<Type> FindActionFilters(this IEnumerable<Type> types)
    {
        var handlers = types
            .Where(type => !type.IsAbstract && type
            .GetTypeInfo()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IActionFilter<,>))
        );
        return handlers;
    }
}