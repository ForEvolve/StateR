using FluentValidation;
using StateR;
using StateR.AfterEffects;
using StateR.Blazor.Persistance;
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