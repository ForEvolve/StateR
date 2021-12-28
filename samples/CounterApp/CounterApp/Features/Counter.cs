using StateR;
using StateR.Updaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CounterApp.Features;

public class Counter
{
    public record State(int Count) : StateBase;

    public class InitialState : IInitialState<State>
    {
        public State Value => new State(0);
    }

    public record Increment : IAction;
    public record Decrement : IAction;

    public class Updaters : IUpdater<Increment, State>, IUpdater<Decrement, State>
    {
        public State Update(Increment action, State state) => state with { Count = state.Count + 1 };
        public State Update(Decrement action, State state) => state with { Count = state.Count - 1 };
    }
}
