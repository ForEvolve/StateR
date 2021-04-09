using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Reducers.Hooks
{
    public class ReducerHooksCollectionTest
    {
        private readonly Mock<IBeforeReducerHook> _before1Mock = new();
        private readonly Mock<IBeforeReducerHook> _before2Mock = new();
        private readonly Mock<IAfterReducerHook> _after1Mock = new();
        private readonly Mock<IAfterReducerHook> _after2Mock = new();

        private readonly Mock<IState<TestState>> _stateMock = new();
        private readonly Mock<IReducer<TestAction, TestState>> _reducer = new();

        private readonly IDispatchContext<TestAction> _dispatchContext = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object);
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        private readonly ReducerHooksCollection sut;

        public ReducerHooksCollectionTest()
        {
            sut = new(
                new[] { _before1Mock.Object, _before2Mock.Object },
                new[] { _after1Mock.Object, _after2Mock.Object }
            );
        }

        public class BeforeReducerAsync : ReducerHooksCollectionTest
        {
            [Fact]
            public async Task Should_call_all_hooks()
            {
                // Act
                await sut.BeforeReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken);

                // Assert
                _before1Mock.Verify(x => x.BeforeReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Once);
                _before2Mock.Verify(x => x.BeforeReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Once);
                _after1Mock.Verify(x => x.AfterReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Never);
                _after2Mock.Verify(x => x.AfterReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Never);
            }
        }

        public class AfterReducerAsync : ReducerHooksCollectionTest
        {
            [Fact]
            public async Task Should_call_all_hooks()
            {
                // Act
                await sut.AfterReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken);

                // Assert
                _before1Mock.Verify(x => x.BeforeReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Never);
                _before2Mock.Verify(x => x.BeforeReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Never);
                _after1Mock.Verify(x => x.AfterReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Once);
                _after2Mock.Verify(x => x.AfterReducerAsync(_dispatchContext, _stateMock.Object, _reducer.Object, _cancellationToken), Times.Once);
            }
        }

        public record TestAction : IAction;
        public record TestState : StateBase;
    }
}
