using Moq;
using Xunit;

namespace StateR.ActionHandlers.Hooks;

public class ActionHandlerHooksCollectionTest
{
    private readonly Mock<IBeforeActionHook> _before1Mock = new();
    private readonly Mock<IBeforeActionHook> _before2Mock = new();
    private readonly Mock<IAfterActionHook> _after1Mock = new();
    private readonly Mock<IAfterActionHook> _after2Mock = new();

    private readonly Mock<IActionHandler<TestAction>> _afterEffectMock = new();
    private readonly IDispatchContext<TestAction> dispatchContext;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly ActionHandlerHooksCollection sut;

    public ActionHandlerHooksCollectionTest()
    {
        dispatchContext = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, _cancellationTokenSource);
        sut = new(
            new[] { _before1Mock.Object, _before2Mock.Object },
            new[] { _after1Mock.Object, _after2Mock.Object }
        );
    }
    public class BeforeHandlerAsync : ActionHandlerHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _before2Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _after1Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _after2Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
        }
    }

    public class AfterHandlerAsync : ActionHandlerHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _before2Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _after1Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _after2Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
        }
    }
    public record TestAction : IAction;
}
