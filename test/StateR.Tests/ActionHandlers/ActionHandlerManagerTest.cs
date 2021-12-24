using Microsoft.Extensions.DependencyInjection;
using Moq;
using StateR.ActionHandlers.Hooks;
using Xunit;

namespace StateR.ActionHandlers;

public class ActionHandlerManagerTest
{
    private readonly Mock<IActionHandlerHooksCollection> _hooksCollectionMock = new();

    protected ActionHandlersManager CreateUpdatersManager(Action<ServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var serviceProvider = services.BuildServiceProvider();
        return new ActionHandlersManager(_hooksCollectionMock.Object, serviceProvider);
    }

    public class DispatchAsync : ActionHandlerManagerTest
    {
        [Fact]
        public async Task Should_call_all_action_handlers()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var handler1 = new Mock<IActionHandler<TestAction>>();
            var handler2 = new Mock<IActionHandler<TestAction>>();
            var sut = CreateUpdatersManager(services =>
            {
                services.AddSingleton(handler1.Object);
                services.AddSingleton(handler2.Object);
            });

            // Act
            await sut.DispatchAsync(context);

            // Assert
            handler1.Verify(x => x.HandleAsync(context, cancellationTokenSource.Token), Times.Once);
            handler2.Verify(x => x.HandleAsync(context, cancellationTokenSource.Token), Times.Once);
        }

        [Fact]
        public async Task Should_break_handlers_when_Cancel()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var afterEffect1 = new Mock<IActionHandler<TestAction>>();
            afterEffect1.Setup(x => x.HandleAsync(context, cancellationTokenSource.Token))
                .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken)
                    => context.Cancel());
            var afterEffect2 = new Mock<IActionHandler<TestAction>>();
            var sut = CreateUpdatersManager(services =>
            {
                services.AddSingleton(afterEffect1.Object);
                services.AddSingleton(afterEffect2.Object);
            });

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(()
                => sut.DispatchAsync(context));

            // Assert
            afterEffect1.Verify(x => x.HandleAsync(context, cancellationTokenSource.Token), Times.Once);
            afterEffect2.Verify(x => x.HandleAsync(context, cancellationTokenSource.Token), Times.Never);
        }

        [Fact]
        public async Task Should_call_middleware_and_handlers_in_order()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var operationQueue = new Queue<string>();
            var actionHandler1 = new Mock<IActionHandler<TestAction>>();
            actionHandler1.Setup(x => x.HandleAsync(context, cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("actionHandler1.HandleAsync"));
            var actionHandler2 = new Mock<IActionHandler<TestAction>>();
            actionHandler2.Setup(x => x.HandleAsync(context, cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("actionHandler2.HandleAsync"));
            _hooksCollectionMock
                .Setup(x => x.BeforeHandlerAsync(context, It.IsAny<IActionHandler<TestAction>>(), cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("BeforeHandlerAsync"));
            _hooksCollectionMock
                .Setup(x => x.AfterHandlerAsync(context, It.IsAny<IActionHandler<TestAction>>(), cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("AfterHandlerAsync"));

            var sut = CreateUpdatersManager(services =>
            {
                services.AddSingleton(actionHandler1.Object);
                services.AddSingleton(actionHandler2.Object);
            });

            // Act
            await sut.DispatchAsync(context);

            // Assert
            Assert.Collection(operationQueue,
                op => Assert.Equal("BeforeHandlerAsync", op),
                op => Assert.Equal("actionHandler1.HandleAsync", op),
                op => Assert.Equal("AfterHandlerAsync", op),

                op => Assert.Equal("BeforeHandlerAsync", op),
                op => Assert.Equal("actionHandler2.HandleAsync", op),
                op => Assert.Equal("AfterHandlerAsync", op)
            );
        }
    }

    public record TestAction : IAction;
}
