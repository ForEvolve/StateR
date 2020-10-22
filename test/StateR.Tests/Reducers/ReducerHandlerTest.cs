using Microsoft.Extensions.DependencyInjection;
using Moq;
using StateR.Reducers.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Reducers
{
    public class ReducerHandlerTest
    {
        private readonly TestState _state = new();
        private readonly Mock<IState<TestState>> _stateMock = new();
        private readonly List<IReducer<TestAction, TestState>> _reducers = new();
        private readonly List<IReducersMiddleware> _middlewares = new();
        private readonly ReducerHandler<TestState, TestAction> sut;
        private readonly Queue<string> _operationQueue = new();
        private readonly TestAction _action = new();
        private readonly DispatchContext<TestAction> _context;
        private readonly CancellationToken _token = CancellationToken.None;

        public ReducerHandlerTest()
        {
            _context = new(_action);

            _stateMock.Setup(x => x.Current).Returns(_state);
            _stateMock.Setup(x => x.Notify())
                .Callback(() => _operationQueue.Enqueue("state.Notify"));

            var reducer1Mock = new Mock<IReducer<TestAction, TestState>>();
            reducer1Mock
                .Setup(x => x.Reduce(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("reducer1.Reduce"));
            _reducers.Add(reducer1Mock.Object);

            var reducer2Mock = new Mock<IReducer<TestAction, TestState>>();
            reducer2Mock
                .Setup(x => x.Reduce(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("reducer2.Reduce"));
            _reducers.Add(reducer2Mock.Object);

            sut = new ReducerHandler<TestState, TestAction>(
                _stateMock.Object,
                _reducers,
                _middlewares
            );
        }

        public class HandleAsync : ReducerHandlerTest
        {
            [Fact]
            public async Task Should_call_reducers_then_notify()
            {
                // Act
                await sut.HandleAsync(_context, _token);

                // Assert
                Assert.Collection(_operationQueue,
                    op => Assert.Equal("reducer1.Reduce", op),
                    op => Assert.Equal("reducer2.Reduce", op),
                    op => Assert.Equal("state.Notify", op)
                );
            }
        }

        [Fact]
        public async Task Should_call_middleware_and_middlewares_methods_in_order()
        {
            // Arrange
            var middleware1 = new TestReducersMiddleware("middleware1", _operationQueue);
            var middleware2 = new TestReducersMiddleware("middleware2", _operationQueue);
            _middlewares.Add(middleware1);
            _middlewares.Add(middleware2);

            // Act
            await sut.HandleAsync(_context, _token);

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("middleware1.BeforeReducersAsync", op),
                op => Assert.Equal("middleware2.BeforeReducersAsync", op),

                op => Assert.Equal("middleware1.BeforeReducerAsync", op),
                op => Assert.Equal("middleware2.BeforeReducerAsync", op),
                op => Assert.Equal("reducer1.Reduce", op),
                op => Assert.Equal("middleware1.AfterReducerAsync", op),
                op => Assert.Equal("middleware2.AfterReducerAsync", op),

                op => Assert.Equal("middleware1.BeforeReducerAsync", op),
                op => Assert.Equal("middleware2.BeforeReducerAsync", op),
                op => Assert.Equal("reducer2.Reduce", op),
                op => Assert.Equal("middleware1.AfterReducerAsync", op),
                op => Assert.Equal("middleware2.AfterReducerAsync", op),

                op => Assert.Equal("middleware1.AfterReducersAsync", op),
                op => Assert.Equal("middleware2.AfterReducersAsync", op),

                op => Assert.Equal("middleware1.BeforeNotifyAsync", op),
                op => Assert.Equal("middleware2.BeforeNotifyAsync", op),
                op => Assert.Equal("state.Notify", op),
                op => Assert.Equal("middleware1.AfterNotifyAsync", op),
                op => Assert.Equal("middleware2.AfterNotifyAsync", op)
            );
        }

        public record TestAction : IAction;
        public record TestState : StateBase;

        private class TestReducersMiddleware : IReducersMiddleware
        {
            private readonly string _name;
            private readonly Queue<string> _operationQueue;
            public TestReducersMiddleware(string name, Queue<string> operationQueue)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                _operationQueue = operationQueue ?? throw new ArgumentNullException(nameof(operationQueue));
            }

            public Task AfterNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.AfterNotifyAsync");
                return Task.CompletedTask;
            }

            public Task AfterReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.AfterReducerAsync");
                return Task.CompletedTask;
            }

            public Task AfterReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.AfterReducersAsync");
                return Task.CompletedTask;
            }

            public Task BeforeNotifyAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.BeforeNotifyAsync");
                return Task.CompletedTask;
            }

            public Task BeforeReducerAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IReducer<TAction, TState> reducer, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.BeforeReducerAsync");
                return Task.CompletedTask;
            }

            public Task BeforeReducersAsync<TAction, TState>(IDispatchContext<TAction> context, IState<TState> state, IEnumerable<IReducer<TAction, TState>> reducers, CancellationToken cancellationToken)
                where TAction : IAction
                where TState : StateBase
            {
                _operationQueue.Enqueue($"{_name}.BeforeReducersAsync");
                return Task.CompletedTask;
            }
        }
    }
}
