using FluentValidation;
using StateR;
//using StateR.Blazor.Persistance;
using StateR.Updaters;

namespace CounterApp.Features;

public class Counter
{
    //[Persist]
    public record class State(int Count) : StateBase;

    public class InitialState : IInitialState<State>
    {
        public State Value => new(0);
    }

    public record class Increment : IAction<State>;
    public record class Decrement : IAction<State>;
    public record class SetPositive(int Count) : IAction<State>;
    public record class SetNegative(int Count) : IAction<State>;
    
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