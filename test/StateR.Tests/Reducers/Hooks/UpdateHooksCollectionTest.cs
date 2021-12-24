using Moq;
using Xunit;

namespace StateR.Updaters.Hooks;

public class UpdateHooksCollectionTest
{
    private readonly Mock<IBeforeUpdateHook> _before1Mock = new();
    private readonly Mock<IBeforeUpdateHook> _before2Mock = new();
    private readonly Mock<IAfterUpdateHook> _after1Mock = new();
    private readonly Mock<IAfterUpdateHook> _after2Mock = new();

    private readonly Mock<IState<TestState>> _stateMock = new();
    private readonly Mock<IUpdater<TestAction, TestState>> _updater = new();

    private readonly IDispatchContext<TestAction> _dispatchContext;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly UpdateHooksCollection sut;

    public UpdateHooksCollectionTest()
    {
        _dispatchContext = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, _cancellationTokenSource);
        sut = new UpdateHooksCollection(
            new[] { _before1Mock.Object, _before2Mock.Object },
            new[] { _after1Mock.Object, _after2Mock.Object }
        );
    }

    public class BeforeUpdateAsync : UpdateHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.BeforeUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Once);
            _before2Mock.Verify(x => x.BeforeUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Once);
            _after1Mock.Verify(x => x.AfterUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Never);
            _after2Mock.Verify(x => x.AfterUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Never);
        }
    }

    public class AfterUpdateAsync : UpdateHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.AfterUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Never);
            _before2Mock.Verify(x => x.BeforeUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Never);
            _after1Mock.Verify(x => x.AfterUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Once);
            _after2Mock.Verify(x => x.AfterUpdateAsync(_dispatchContext, _stateMock.Object, _updater.Object, _cancellationToken), Times.Once);
        }
    }

    public record TestAction : IAction;
    public record TestState : StateBase;
}
