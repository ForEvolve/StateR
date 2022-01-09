using FluentValidation;
using StateR;
using StateR.AfterEffects;
using StateR.Blazor.WebStorage;
using StateR.Interceptors;
using StateR.Internal;
using StateR.Updaters;
using System;
using System.Reflection;

namespace CounterApp.Features;

public class Counter
{
    [Persist]
    public record class State(int Count) : StateBase;

    public class InitialState : IInitialState<State>
    {
        public State Value => new(0);
    }

    public record class Increment : IAction;
    public record class Decrement : IAction;
    public record class SetPositive(int Count) : IAction;
    public record class SetNegative(int Count) : IAction;
    
    public class Updaters : IUpdater<Increment, State>, IUpdater<Decrement, State>, IUpdater<SetPositive, State>, IUpdater<SetNegative, State>
    {
        public State Update(Increment action, State state)
            => state with { Count = state.Count + 1 };
        public State Update(Decrement action, State state)
            => state with { Count = state.Count - 1 };
        public State Update(SetPositive action, State state)
            => state with { Count = action.Count };
        public State Update(SetNegative action, State state)
            => state with { Count = action.Count };
    }

    public class SetPositiveValidator : AbstractValidator<SetPositive>
    {
        public SetPositiveValidator()
        {
            RuleFor(x => x.Count).GreaterThan(0);
        }
    }

    public class SetNegativeValidator : AbstractValidator<SetNegative>
    {
        public SetNegativeValidator()
        {
            RuleFor(x => x.Count).LessThan(0);
        }
    }

    public class StateValidator : AbstractValidator<State>
    {
        public StateValidator()
        {
            RuleFor(x => x.Count).GreaterThan(-100);
            RuleFor(x => x.Count).LessThan(100);
        }
    }
}

//
// Persistence experimentations
//
[AttributeUsage(AttributeTargets.Class)]
public class PersistAttribute : Attribute
{
    public PersistenceType Type { get; set; } = PersistenceType.SessionStorage;
}

public enum PersistenceType
{
    SessionStorage,
    LocalStorage
}

public class SessionStateDecorator<TState> : IState<TState>
    where TState : StateBase
{
    private readonly IStorage _storage;
    private readonly IState<TState> _next;
    private readonly string _key;

    public SessionStateDecorator(IWebStorage webStorage, IState<TState> next)
    {
        ArgumentNullException.ThrowIfNull(webStorage, nameof(webStorage));
        _storage = webStorage.LocalStorage;
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _key = typeof(TState).GetStatorName();
    }

    public void Set(TState state)
    {
        _next.Set(state);
        Console.WriteLine($"[SessionStateDecorator][{_key}] Set: {state}.");
        _storage.SetItem(_key, state);
    }

    public TState Current => _next.Current;
    public void Notify() => _next.Notify();
    public void Subscribe(Action stateHasChangedDelegate)
        => _next.Subscribe(stateHasChangedDelegate);
    public void Unsubscribe(Action stateHasChangedDelegate)
        => _next.Unsubscribe(stateHasChangedDelegate);
}

public class InitialSessionStateDecorator<TState> : IInitialState<TState>
    where TState : StateBase
{
    private readonly IStorage _storage;
    private readonly IInitialState<TState> _next;
    private readonly string _key;

    public InitialSessionStateDecorator(IWebStorage webStorage, IInitialState<TState> next)
    {
        ArgumentNullException.ThrowIfNull(webStorage, nameof(webStorage));
        _storage = webStorage.LocalStorage;
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _key = typeof(TState).GetStatorName();
    }

    public TState Value
    {
        get
        {
            var item = _storage.GetItem<TState>(_key);
            if (item == null)
            {
                Console.WriteLine($"[InitialSessionStateDecorator][{_key}] Not item found in storage.");
                return _next.Value;
            }
            Console.WriteLine($"[InitialSessionStateDecorator][{_key}] Item found: {item}");
            return item;
        }
    }
}

public static class PersistenceStartupExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, params Assembly[] assembliesToScan)
    {
        ArgumentNullException.ThrowIfNull(assembliesToScan, nameof(assembliesToScan));
        if (assembliesToScan.Length == 0) { throw new ArgumentOutOfRangeException(nameof(assembliesToScan)); }

        var states = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(StateBase)));
        ;
        foreach (var state in states)
        {
            var persistAttribute = state.GetCustomAttribute<PersistAttribute>();
            if (persistAttribute == null)
            {
                continue;
            }
            // TODO: do something with Type
            //switch (persistAttribute.Type)
            //{
            //    case PersistanceType.SessionStorage:
            //        break;
            //}

            Console.WriteLine($"Persistence ({persistAttribute.Type}): {state.GetStatorName()}");
            Console.WriteLine($"- Decorate<IInitialState<{state.GetStatorName()}>, InitialSessionStateDecorator<{state.GetStatorName()}>>()");

            // Equivalent to: Decorate<IInitialState<TState>, InitialSessionStateDecorator<TState>>();
            var initialStateType = typeof(IInitialState<>).MakeGenericType(state);
            var decoratedInitialStateType = typeof(InitialSessionStateDecorator<>).MakeGenericType(state);
            services.Decorate(initialStateType, decoratedInitialStateType);

            Console.WriteLine($"- Decorate<IState<{state.GetStatorName()}>, SessionStateDecorator<{state.GetStatorName()}>>()");

            // Equivalent to: Decorate<IState<TState>, SessionStateDecorator<TState>>();
            var stateType = typeof(IState<>).MakeGenericType(state);
            var stateSessionDecoratorType = typeof(SessionStateDecorator<>).MakeGenericType(state);
            services.Decorate(stateType, stateSessionDecoratorType);
        }
        return services;
    }
}