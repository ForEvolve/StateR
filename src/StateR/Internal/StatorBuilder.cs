﻿using Microsoft.Extensions.DependencyInjection;

namespace StateR.Internal;

public class StatorBuilder : IStatorBuilder
{
    public StatorBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IStatorBuilder AddTypes(IEnumerable<Type> types)
        => AddDistinctTypes(All, types);
    public IStatorBuilder AddStates(IEnumerable<Type> types)
        => AddDistinctTypes(States, types);
    public IStatorBuilder AddActions(IEnumerable<Type> types)
        => AddDistinctTypes(Actions, types);
    public IStatorBuilder AddUpdaters(IEnumerable<Type> types)
        => AddDistinctTypes(Updaters, types);
    public IStatorBuilder AddActionHandlers(IEnumerable<Type> types)
        => AddDistinctTypes(ActionHandlers, types);

    public IServiceCollection Services { get; }
    public List<Type> Actions { get; } = new List<Type>();
    public List<Type> States { get; } = new List<Type>();
    public List<Type> Interceptors { get; } = new List<Type>();
    public List<Type> ActionHandlers { get; } = new List<Type>();
    public List<Type> AfterEffects { get; } = new List<Type>();
    public List<Type> Updaters { get; } = new List<Type>();
    public List<Type> All { get; } = new List<Type>();

    private IStatorBuilder AddDistinctTypes(List<Type> list, IEnumerable<Type> types)
    {
        var distinctTypes = types.Except(list).Distinct();
        list.AddRange(distinctTypes);
        return this;
    }
}
