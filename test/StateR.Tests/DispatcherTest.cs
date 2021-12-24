using Moq;
using StateR.ActionHandlers;
using StateR.AfterEffects;
using StateR.Interceptors;
using Xunit;

namespace StateR;

public class DispatcherTest
{
    private readonly Mock<IDispatchContextFactory> _dispatchContextFactory = new();
    private readonly Mock<IInterceptorsManager> _interceptorsManager = new();
    private readonly Mock<IActionHandlersManager> _actionHandlersManager = new();
    private readonly Mock<IAfterEffectsManager> _afterEffectsManager = new();
    private readonly Dispatcher sut;

    public DispatcherTest()
    {
        sut = new(_dispatchContextFactory.Object, _interceptorsManager.Object, _actionHandlersManager.Object, _afterEffectsManager.Object);
    }

    public class DispatchAsync : DispatcherTest
    {
        [Fact]
        public async Task Should_create_DispatchContext_using_dispatchContextFactory()
        {
            var action = new TestAction();
            await sut.DispatchAsync(action, CancellationToken.None);
            _dispatchContextFactory
                .Verify(x => x.Create(action, sut, It.IsAny<CancellationTokenSource>()), Times.Once);
        }

        [Fact]
        public async Task Should_send_the_same_DispatchContext_to_all_managers()
        {
            // Arrange
            var action = new TestAction();
            var context = new DispatchContext<TestAction>(action, new Mock<IDispatcher>().Object, new CancellationTokenSource());
            _dispatchContextFactory
                .Setup(x => x.Create(action, sut, It.IsAny<CancellationTokenSource>()))
                .Returns(context);

            // Act
            await sut.DispatchAsync(action, CancellationToken.None);

            // Assert
            _interceptorsManager.Verify(x => x.DispatchAsync(context), Times.Once);
            _actionHandlersManager.Verify(x => x.DispatchAsync(context), Times.Once);
            _afterEffectsManager.Verify(x => x.DispatchAsync(context), Times.Once);
        }
        [Fact]
        public async Task Should_call_managers_in_the_expected_order()
        {
            // Arrange
            var action = new TestAction();
            var operationQueue = new Queue<string>();
            _interceptorsManager
                .Setup(x => x.DispatchAsync(It.IsAny<IDispatchContext<TestAction>>()))
                .Callback(() => operationQueue.Enqueue("Interceptors"));
            _actionHandlersManager
                .Setup(x => x.DispatchAsync(It.IsAny<IDispatchContext<TestAction>>()))
                .Callback(() => operationQueue.Enqueue("Updaters"));
            _afterEffectsManager
                .Setup(x => x.DispatchAsync(It.IsAny<IDispatchContext<TestAction>>()))
                .Callback(() => operationQueue.Enqueue("AfterEffects"));

            // Act
            await sut.DispatchAsync(action, CancellationToken.None);

            // Assert
            Assert.Collection(operationQueue,
                operation => Assert.Equal("Interceptors", operation),
                operation => Assert.Equal("Updaters", operation),
                operation => Assert.Equal("AfterEffects", operation)
            );
        }
    }

    private record TestAction : IAction;
}
