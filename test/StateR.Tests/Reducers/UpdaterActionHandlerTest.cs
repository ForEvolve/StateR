using Moq;
using StateR.Updaters.Hooks;
using Xunit;

namespace StateR.Updaters;

public class UpdaterActionHandlerTest
{
    private readonly TestState _state = new();
    private readonly Mock<IState<TestState>> _stateMock = new();
    private readonly List<IUpdater<TestAction, TestState>> _updaters = new();
    private readonly Mock<IUpdateHooksCollection> _hooksMock = new();
    private readonly UpdaterMiddleware<TestState, TestAction> sut;
    private readonly Queue<string> _operationQueue = new();
    private readonly TestAction _action = new();
    private readonly DispatchContext<TestAction> _context;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly Mock<IUpdater<TestAction, TestState>> _updater1Mock = new();
    private readonly Mock<IUpdater<TestAction, TestState>> _updater2Mock = new();

    public UpdaterActionHandlerTest()
    {
        _context = new(_action, new Mock<IDispatcher>().Object, _cancellationTokenSource);

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

        sut = new UpdaterActionHandler<TestState, TestAction>(
            _stateMock.Object,
            _updaters,
            _hooksMock.Object
        );
    }

    public class HandleAsync : UpdaterActionHandlerTest
    {
        [Fact]
        public async Task Should_call_updaters_then_notify()
        {
            // Act
            await sut.HandleAsync(_context, CancellationToken.None);

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("updater1.Update", op),
                op => Assert.Equal("updater2.Update", op),
                op => Assert.Equal("state.Notify", op)
            );
        }

        [Fact]
        public async Task Should_break_updates_when_Cancel_is_called_in_a_BeforeUpdateAsync_hook()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _cancellationTokenSource.Token))
                .Callback(() =>
                {
                    _operationQueue.Enqueue("BeforeUpdaterAsync:Updater2");
                    _cancellationTokenSource.Cancel();
                });
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater2"));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(()
                => sut.HandleAsync(_context, _cancellationTokenSource.Token));

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("BeforeUpdaterAsync:Updater1", op),
                op => Assert.Equal("updater1.Update", op),
                op => Assert.Equal("AfterUpdaterAsync:Updater1", op),

                op => Assert.Equal("BeforeUpdaterAsync:Updater2", op),

                op => Assert.Equal("state.Notify", op)
            );
        }

        [Fact]
        public async Task Should_break_updates_when_Cancel_is_called_in_an_AfterUpdateAsync_hook()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater2"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, _cancellationTokenSource.Token))
                .Callback(() =>
                {
                    _operationQueue.Enqueue("AfterUpdaterAsync:Updater1");
                    _cancellationTokenSource.Cancel();
                });
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, _cancellationTokenSource.Token))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater2"));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(()
                => sut.HandleAsync(_context, _cancellationTokenSource.Token));

            // Assert
            Assert.Collection(_operationQueue,
                op => Assert.Equal("BeforeUpdaterAsync:Updater1", op),
                op => Assert.Equal("updater1.Update", op),
                op => Assert.Equal("AfterUpdaterAsync:Updater1", op),

                op => Assert.Equal("state.Notify", op)
            );
        }

        [Fact]
        public async Task Should_call_hooks_methods_in_order()
        {
            // Arrange
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, CancellationToken.None))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.BeforeUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, CancellationToken.None))
                .Callback(() => _operationQueue.Enqueue("BeforeUpdaterAsync:Updater2"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater1Mock.Object, CancellationToken.None))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater1"));
            _hooksMock
                .Setup(x => x.AfterUpdateAsync(_context, _stateMock.Object, _updater2Mock.Object, CancellationToken.None))
                .Callback(() => _operationQueue.Enqueue("AfterUpdaterAsync:Updater2"));

            // Act
            await sut.HandleAsync(_context, CancellationToken.None);

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
    }


    public record TestAction : IAction;
    public record TestState : StateBase;
}
