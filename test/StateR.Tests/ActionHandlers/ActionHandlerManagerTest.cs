using Microsoft.Extensions.DependencyInjection;
using Moq;
using StateR.ActionHandlers.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.ActionHandlers
{
    public class ActionHandlerManagerTest
    {
        private readonly Mock<IActionHandlerHooksCollection> _hooksCollectionMock = new();

        protected ActionHandlersManager CreateReducersManager(Action<ServiceCollection> configureServices)
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
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var handler1 = new Mock<IActionHandler<TestAction>>();
                var handler2 = new Mock<IActionHandler<TestAction>>();
                var sut = CreateReducersManager(services =>
                {
                    services.AddSingleton(handler1.Object);
                    services.AddSingleton(handler2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                handler1.Verify(x => x.HandleAsync(context, token), Times.Once);
                handler2.Verify(x => x.HandleAsync(context, token), Times.Once);
            }

            [Fact]
            public async Task Should_break_handlers_when_StopReduce_is_true()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var afterEffect1 = new Mock<IActionHandler<TestAction>>();
                afterEffect1.Setup(x => x.HandleAsync(context, token))
                    .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken) => context.StopReduce = true);
                var afterEffect2 = new Mock<IActionHandler<TestAction>>();
                var sut = CreateReducersManager(services =>
                {
                    services.AddSingleton(afterEffect1.Object);
                    services.AddSingleton(afterEffect2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                afterEffect1.Verify(x => x.HandleAsync(context, token), Times.Once);
                afterEffect2.Verify(x => x.HandleAsync(context, token), Times.Never);
            }

            [Fact]
            public async Task Should_call_middleware_and_handlers_in_order()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var operationQueue = new Queue<string>();
                var actionHandler1 = new Mock<IActionHandler<TestAction>>();
                actionHandler1.Setup(x => x.HandleAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("actionHandler1.HandleAsync"));
                var actionHandler2 = new Mock<IActionHandler<TestAction>>();
                actionHandler2.Setup(x => x.HandleAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("actionHandler2.HandleAsync"));
                _hooksCollectionMock
                    .Setup(x => x.BeforeHandlerAsync(context, It.IsAny<IActionHandler<TestAction>>(), token))
                    .Callback(() => operationQueue.Enqueue("BeforeHandlerAsync"));
                _hooksCollectionMock
                    .Setup(x => x.AfterHandlerAsync(context, It.IsAny<IActionHandler<TestAction>>(), token))
                    .Callback(() => operationQueue.Enqueue("AfterHandlerAsync"));

                var sut = CreateReducersManager(services =>
                {
                    services.AddSingleton(actionHandler1.Object);
                    services.AddSingleton(actionHandler2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

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
}
