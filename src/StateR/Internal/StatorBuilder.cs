using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace StateR.Internal;

public class StatorBuilder : IStatorBuilder
{
    private readonly List<Type> _states = new();
    private readonly List<Type> _initialStates = new();

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
        => AddDistinctTypes(Actions, types);
    public IStatorBuilder AddUpdaters(IEnumerable<Type> types)
        => AddDistinctTypes(Updaters, types);
    public IStatorBuilder AddActionHandlers(IEnumerable<Type> types)
        => AddDistinctTypes(ActionHandlers, types);

    public IServiceCollection Services { get; }
    public List<Type> Actions { get; } = new List<Type>();
    public List<Type> Interceptors { get; } = new List<Type>();
    public List<Type> ActionHandlers { get; } = new List<Type>();
    public List<Type> AfterEffects { get; } = new List<Type>();
    public List<Type> Updaters { get; } = new List<Type>();
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
