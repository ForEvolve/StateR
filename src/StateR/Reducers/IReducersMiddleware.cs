using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace StateR.Reducers
{
    public interface IReducersMiddleware
    {
        Task BeforeReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task BeforeNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
        Task AfterNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
            where TAction : IAction
            where TState : StateBase;
    }
}
