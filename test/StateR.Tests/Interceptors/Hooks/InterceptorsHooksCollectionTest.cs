using Moq;
using Xunit;

namespace StateR.Interceptors.Hooks;

public class InterceptorsHooksCollectionTest
{
    private readonly Mock<IBeforeInterceptorHook> _before1Mock = new();
    private readonly Mock<IBeforeInterceptorHook> _before2Mock = new();
    private readonly Mock<IAfterInterceptorHook> _after1Mock = new();
    private readonly Mock<IAfterInterceptorHook> _after2Mock = new();

    private readonly Mock<IInterceptor<TestAction>> _afterEffectMock = new();
    private readonly IDispatchContext<TestAction> _dispatchContext;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly InterceptorsHooksCollection sut;

    public InterceptorsHooksCollectionTest()
    {
        _dispatchContext = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, _cancellationTokenSource);
        sut = new(
            new[] { _before1Mock.Object, _before2Mock.Object },
            new[] { _after1Mock.Object, _after2Mock.Object }
        );
    }

    public class BeforeHandlerAsync : InterceptorsHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.BeforeHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _before2Mock.Verify(x => x.BeforeHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _after1Mock.Verify(x => x.AfterHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _after2Mock.Verify(x => x.AfterHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
        }
    }
    public class AfterHandlerAsync : InterceptorsHooksCollectionTest
    {
        [Fact]
        public async Task Should_call_all_hooks()
        {
            // Act
            await sut.AfterHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken);

            // Assert
            _before1Mock.Verify(x => x.BeforeHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _before2Mock.Verify(x => x.BeforeHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            _after1Mock.Verify(x => x.AfterHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            _after2Mock.Verify(x => x.AfterHandlerAsync(_dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
        }
    }
    public record TestAction : IAction;
}
