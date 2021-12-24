using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StateR.AfterEffects.Hooks;
using Xunit;

namespace StateR.AfterEffects;

public class AfterEffectsManagerTest
{
    private readonly Mock<IAfterEffectHooksCollection> _afterEffectHooksCollectionMock = new();
    protected AfterEffectsManager CreateAfterEffectsManager(Action<ServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        services.TryAddSingleton(_afterEffectHooksCollectionMock.Object);
        var serviceProvider = services.BuildServiceProvider();
        var afterEffectHooksCollection = serviceProvider.GetService<IAfterEffectHooksCollection>();
        return new AfterEffectsManager(afterEffectHooksCollection, serviceProvider);
    }

    public class DispatchAsync : AfterEffectsManagerTest
    {
        [Fact]
        public async Task Should_handle_all_after_effects()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var afterEffect1 = new Mock<IAfterEffects<TestAction>>();
            var afterEffect2 = new Mock<IAfterEffects<TestAction>>();
            var sut = CreateAfterEffectsManager(services =>
            {
                services.AddSingleton(afterEffect1.Object);
                services.AddSingleton(afterEffect2.Object);
            });

            // Act
            await sut.DispatchAsync(context);

            // Assert
            afterEffect1.Verify(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token), Times.Once);
            afterEffect2.Verify(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token), Times.Once);
        }

        [Fact]
        public async Task Should_break_after_effects_when_Cancel()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var afterEffect1 = new Mock<IAfterEffects<TestAction>>();
            afterEffect1.Setup(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token))
                .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken) => context.Cancel());
            var afterEffect2 = new Mock<IAfterEffects<TestAction>>();
            var sut = CreateAfterEffectsManager(services =>
            {
                services.AddSingleton(afterEffect1.Object);
                services.AddSingleton(afterEffect2.Object);
            });

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(()
                => sut.DispatchAsync(context));

            // Assert
            afterEffect1.Verify(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token), Times.Once);
            afterEffect2.Verify(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token), Times.Never);
        }

        [Fact]
        public async Task Should_call_hooks_and_after_effects_methods_in_order()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

            var operationQueue = new Queue<string>();
            var afterEffect1 = new Mock<IAfterEffects<TestAction>>();
            afterEffect1.Setup(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("afterEffect1.HandleAfterEffectAsync"));
            var afterEffect2 = new Mock<IAfterEffects<TestAction>>();
            afterEffect2.Setup(x => x.HandleAfterEffectAsync(context, cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("afterEffect2.HandleAfterEffectAsync"));
            _afterEffectHooksCollectionMock
                .Setup(x => x.BeforeHandlerAsync(context, It.IsAny<IAfterEffects<TestAction>>(), cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("BeforeHandlerAsync"));
            _afterEffectHooksCollectionMock
                .Setup(x => x.AfterHandlerAsync(context, It.IsAny<IAfterEffects<TestAction>>(), cancellationTokenSource.Token))
                .Callback(() => operationQueue.Enqueue("AfterHandlerAsync"));
            var sut = CreateAfterEffectsManager(services =>
            {
                services.AddSingleton(afterEffect1.Object);
                services.AddSingleton(afterEffect2.Object);
            });

            // Act
            await sut.DispatchAsync(context);

            // Assert
            Assert.Collection(operationQueue,
                op => Assert.Equal("BeforeHandlerAsync", op),
                op => Assert.Equal("afterEffect1.HandleAfterEffectAsync", op),
                op => Assert.Equal("AfterHandlerAsync", op),

                op => Assert.Equal("BeforeHandlerAsync", op),
                op => Assert.Equal("afterEffect2.HandleAfterEffectAsync", op),
                op => Assert.Equal("AfterHandlerAsync", op)
            );
        }
    }

    public record TestAction : IAction;
}
