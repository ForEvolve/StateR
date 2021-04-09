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
        private readonly Mock<IReducerHooksCollection> _hooksMock = new();
        private readonly ReducerHandler<TestState, TestAction> sut;
        private readonly Queue<string> _operationQueue = new();
        private readonly TestAction _action = new();
        private readonly DispatchContext<TestAction> _context;
        private readonly CancellationToken _token = CancellationToken.None;

        private readonly Mock<IReducer<TestAction, TestState>> _reducer1Mock = new();
        private readonly Mock<IReducer<TestAction, TestState>> _reducer2Mock = new();

        public ReducerHandlerTest()
        {
            _context = new(_action, new Mock<IDispatcher>().Object);

            _stateMock.Setup(x => x.Current).Returns(_state);
            _stateMock.Setup(x => x.Notify())
                .Callback(() => _operationQueue.Enqueue("state.Notify"));

            _reducer1Mock
                .Setup(x => x.Reduce(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("reducer1.Reduce"));
            _reducers.Add(_reducer1Mock.Object);
            _reducer2Mock
                .Setup(x => x.Reduce(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("reducer2.Reduce"));
            _reducers.Add(_reducer2Mock.Object);

            sut = new ReducerHandler<TestState, TestAction>(
                _stateMock.Object,
                _reducers,
                _hooksMock.Object
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
            _hooksMock
                .Setup(x => x.BeforeReducerAsync(_context, _stateMock.Object, _reducer1Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("BeforeReducerAsync:Reducer1"));
            _hooksMock
                .Setup(x => x.BeforeReducerAsync(_context, _stateMock.Object, _reducer2Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("BeforeReducerAsync:Reducer2"));
            _hooksMock
                .Setup(x => x.AfterReducerAsync(_context, _stateMock.Object, _reducer1Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("AfterReducerAsync:Reducer1"));
            _hooksMock
                .Setup(x => x.AfterReducerAsync(_context, _stateMock.Object, _reducer2Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("AfterReducerAsync:Reducer2"));

            // Act
            await sut.HandleAsync(_context, _token);

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("BeforeReducerAsync:Reducer1", op),
                op => Assert.Equal("reducer1.Reduce", op),
                op => Assert.Equal("AfterReducerAsync:Reducer1", op),

                op => Assert.Equal("BeforeReducerAsync:Reducer2", op),
                op => Assert.Equal("reducer2.Reduce", op),
                op => Assert.Equal("AfterReducerAsync:Reducer2", op),

                op => Assert.Equal("state.Notify", op)
            );
        }

        public record TestAction : IAction;
        public record TestState : StateBase;
    }
}
