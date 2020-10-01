﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR
{
    public class ReducerHandler<TState, TAction> : IActionHandler<TAction>
        where TState : StateBase
        where TAction : IAction
    {
        private readonly IEnumerable<IReducer<TAction, TState>> _reducers;
        private readonly IState<TState> _state;

        public ReducerHandler(IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _reducers = reducers ?? throw new ArgumentNullException(nameof(reducers));
        }

        public void Handle(TAction action)
        {
            foreach (var reducer in _reducers)
            {
                _state.Transform(state => reducer.Reduce(action, state));
            }
            _state.Notify();
        }

        public void Handle(DispatchContext<TAction> context)
        {
            foreach (var reducer in _reducers)
            {
                _state.Transform(state => reducer.Reduce(context.Action, state));
            }
            _state.Notify();
        }
    }
}