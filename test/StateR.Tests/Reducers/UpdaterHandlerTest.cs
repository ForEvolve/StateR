using Microsoft.Extensions.DependencyInjection;
using Moq;
using StateR.Updater.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Updater
{
    public class UpdaterHandlerTest
    {
        private readonly TestState _state = new();
        private readonly Mock<IState<TestState>> _stateMock = new();
        private readonly List<IUpdater<TestAction, TestState>> _updaters = new();
        private readonly Mock<IUpdateHooksCollection> _hooksMock = new();
        private readonly UpdaterHandler<TestState, TestAction> sut;
        private readonly Queue<string> _operationQueue = new();
        private readonly TestAction _action = new();
        private readonly DispatchContext<TestAction> _context;
        private readonly CancellationToken _token = CancellationToken.None;

        private readonly Mock<IUpdater<TestAction, TestState>> _updater1Mock = new();
        private readonly Mock<IUpdater<TestAction, TestState>> _updater2Mock = new();

        public UpdaterHandlerTest()
        {
            _context = new(_action, new Mock<IDispatcher>().Object);

            _stateMock.Setup(x => x.Current).Returns(_state);
            _stateMock.Setup(x => x.Notify())
                .Callback(() => _operationQueue.Enqueue("state.Notify"));

            _updater1Mock
                .Setup(x => x.Update(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("updater1.Update"));
            _updaters.Add(_updater1Mock.Object);
            _updater2Mock
                .Setup(x => x.Update(_action, _state))
                .Returns(_state)
                .Callback(() => _operationQueue.Enqueue("updater2.Update"));
            _updaters.Add(_updater2Mock.Object);

            sut = new UpdaterHandler<TestState, TestAction>(
                _stateMock.Object,
                _updaters,
                _hooksMock.Object
            );
        }

        public class HandleAsync : UpdaterHandlerTest
        {
            [Fact]
            public async Task Should_call_updaters_then_notify()
            {
                // Act
                await sut.HandleAsync(_context, _token);

                // Assert
                Assert.Collection(_operationQueue,
                    op => Assert.Equal("updater1.Update", op),
                    op => Assert.Equal("updater2.Update", op),
                    op => Assert.Equal("state.Notify", op)
                );
            }
        }

        [Fact]
        public async Task Should_call_middleware_and_middlewares_methods_in_order()
        {
            // Arrange
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater2"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _token))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater2"));

            // Act
            await sut.HandleAsync(_context, _token);

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("BeforeUpdaterAsync:Updater1", op),
                op => Assert.Equal("updater1.Update", op),
                op => Assert.Equal("AfterUpdaterAsync:Updater1", op),

                op => Assert.Equal("BeforeUpdaterAsync:Updater2", op),
                op => Assert.Equal("updater2.Update", op),
                op => Assert.Equal("AfterUpdaterAsync:Updater2", op),

                op => Assert.Equal("state.Notify", op)
            );
        }

        public record TestAction : IAction;
        public record TestState : StateBase;
    }
}
