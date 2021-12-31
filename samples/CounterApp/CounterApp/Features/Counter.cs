using FluentValidation;
using StateR;
using StateR.Updaters;

namespace CounterApp.Features;

public class Counter
{
    public record State(int Count) : StateBase;

    public class InitialState : IInitialState<State>
    {
        public State Value => new(0);
    }

    public record Increment : IAction;
    public record Decrement : IAction;
    public record SetPositive(int Count) : IAction;

    public class Updaters : IUpdater<Increment, State>, IUpdater<Decrement, State>, IUpdater<SetPositive, State>
    {
        public State Update(Increment action, State state)
            => state with { Count = state.Count + 1 };
        public State Update(Decrement action, State state)
            => state with { Count = state.Count - 1 };
        public State Update(SetPositive action, State state)
            => state with { Count = action.Count };
    }

    public class SetValidator : AbstractValidator<SetPositive>
    {
        public SetValidator()
        {
            RuleFor(x => x.Count).GreaterThan(0);
        }
    }
}
