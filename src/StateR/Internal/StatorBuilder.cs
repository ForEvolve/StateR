using Microsoft.Extensions.DependencyInjection;
using StateR.Updaters;
using System.Collections.ObjectModel;

namespace StateR.Internal;

public class StatorBuilder : IStatorBuilder
{
    private readonly List<Type> _states = new();
    private readonly List<Type> _initialStates = new();
    private readonly List<Type> _actions = new();
    private readonly List<Type> _updaters = new();

    public StatorBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }


    #region IOldStatorBuilder

    public IStatorBuilder AddTypes(IEnumerable<Type> types)
        => AddDistinctTypes(All, types);
    public IStatorBuilder AddStates(IEnumerable<Type> types)
        => AddDistinctTypes(_states, types);
    public IStatorBuilder AddActions(IEnumerable<Type> types)
        => AddDistinctTypes(_actions, types);
    public IStatorBuilder AddUpdaters(IEnumerable<Type> types)
        => AddDistinctTypes(_updaters, types);
    public IStatorBuilder AddActionHandlers(IEnumerable<Type> types)
        => AddDistinctTypes(ActionHandlers, types);

    public IServiceCollection Services { get; }
    public List<Type> Interceptors { get; } = new List<Type>();
    public List<Type> ActionHandlers { get; } = new List<Type>();
    public List<Type> AfterEffects { get; } = new List<Type>();
    public List<Type> All { get; } = new List<Type>();

    public IStatorBuilder AddMiddlewares(IEnumerable<Type> types)
        => AddDistinctTypes(Middlewares, types);
    public List<Type> Middlewares { get; } = new List<Type>();

    private IStatorBuilder AddDistinctTypes(List<Type> list, IEnumerable<Type> types)
    {
        var distinctTypes = types.Except(list).Distinct();
        list.AddRange(distinctTypes);
        return this;
    }

    #endregion

    public ReadOnlyCollection<Type> States => new(_states);
    public ReadOnlyCollection<Type> InitialStates => new(_initialStates);
    public ReadOnlyCollection<Type> Actions => new(_actions);
    public ReadOnlyCollection<Type> Updaters => new(_updaters);

    public IStatorBuilder AddState<TState, TInitialState>()
        where TState : StateBase
        where TInitialState : IInitialState<TState>
    {
        _states.Add(typeof(TState));
        _initialStates.Add(typeof(TInitialState));
        return this;
    }

    public IStatorBuilder AddState(Type state, Type initialState)
    {
        if (!state.IsAssignableTo(typeof(StateBase)))
        {
            throw new InvalidStateException(state);
        }
        if (!initialState.IsAssignableTo(typeof(IInitialState<>).MakeGenericType(state)))
        {
            throw new InvalidInitialStateException(state, initialState);
        }
        _states.Add(state);
        return this;
    }

    public IStatorBuilder AddAction(Type actionType)
    {
        if(!IsAction(actionType))
        {
            throw new InvalidActionException(actionType);
        }
        _actions.Add(actionType);
        return this;
    }

    public IStatorBuilder AddUpdaters(Type updaterType)
    {
        if(!IsUpdater(updaterType))
        {
            throw new InvalidUpdaterException(updaterType);
        }
        _updaters.Add(updaterType);
        return this;
    }

    private static readonly Type _iActionType = typeof(IAction<>);
    private static bool IsAction(Type actionType)
    {
        var interfaces = actionType.GetInterfaces()
            .Count(i => i.IsGenericType && i.GetGenericTypeDefinition() == _iActionType);
        return interfaces > 0;
    }

    private static readonly Type _iUpdaterType = typeof(IUpdater<,>);
    private static bool IsUpdater(Type updaterType)
    {
        var interfaces = updaterType.GetInterfaces()
            .Count(i => i.IsGenericType && i.GetGenericTypeDefinition() == _iUpdaterType);
        return interfaces > 0;
    }

}

public class InvalidStateException : Exception
{
    public InvalidStateException(Type stateType)
    {
        StateType = stateType;
    }

    public Type StateType { get; }
}

public class InvalidInitialStateException : Exception
{
    public InvalidInitialStateException(Type stateType, Type initialState)
        : base($"The type {initialState.Name} is not a valid IInitialState<{stateType.Name}>.")
    {
        StateType = stateType;
        InitialStateType = initialState;
    }

    public Type StateType { get; }
    public Type InitialStateType { get; }
}

public class InvalidActionException : Exception
{
    public InvalidActionException(Type actionType)
        : base($"The type {actionType.Name} is not a valid IAction<TState>.")
    {
        ActionType = actionType;
    }

    public Type ActionType { get; }
}

public class InvalidUpdaterException : Exception
{
    public InvalidUpdaterException(Type updaterType)
        : base($"The type {updaterType.Name} is not a valid IUpdater<TAction, TState>.")
    {
        UpdaterType = updaterType;
    }

    public Type UpdaterType { get; }
}