using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public class ReducerHandler<TState, TAction> : RequestHandler<TAction>
        where TState : StateBase
        where TAction : IAction
    {
        private readonly IEnumerable<IReducer<TState, TAction>> _reducers;
        private readonly IState<TState> _state;

        public ReducerHandler(IState<TState> state, IEnumerable<IReducer<TState, TAction>> reducers)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
        }

        protected override void Handle(TAction action)
        {
            foreach (var reducer in _reducers)
            {
                _state.Transform(state => reducer.Reduce(action, state));
            }
            _state.Notify();
        }
    }
}