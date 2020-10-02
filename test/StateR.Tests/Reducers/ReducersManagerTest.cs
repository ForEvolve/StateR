using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Reducers
{
    public class ReducersManagerTest
    {
        protected ReducersManager CreateReducersManager(Action<ServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            var serviceProvider = services.BuildServiceProvider();
            var middlewares = serviceProvider.GetServices<IActionHandlerMiddleware>();
            return new ReducersManager(middlewares, serviceProvider);
        }

        public class DispatchAsync : ReducersManagerTest
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
                var middleware1 = new QueueActionHandlerMiddleware("middleware1", operationQueue);
                var middleware2 = new QueueActionHandlerMiddleware("middleware2", operationQueue);
                var sut = CreateReducersManager(services =>
                {
                    services.AddSingleton(actionHandler1.Object);
                    services.AddSingleton(actionHandler2.Object);
                    services.AddSingleton<IActionHandlerMiddleware>(middleware1);
                    services.AddSingleton<IActionHandlerMiddleware>(middleware2);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                Assert.Collection(operationQueue,
                    op => Assert.Equal("middleware1.BeforeHandlersAsync", op),
                    op => Assert.Equal("middleware2.BeforeHandlersAsync", op),

                    op => Assert.Equal("middleware1.BeforeHandlerAsync", op),
                    op => Assert.Equal("middleware2.BeforeHandlerAsync", op),
                    op => Assert.Equal("actionHandler1.HandleAsync", op),
                    op => Assert.Equal("middleware1.AfterHandlerAsync", op),
                    op => Assert.Equal("middleware2.AfterHandlerAsync", op),

                    op => Assert.Equal("middleware1.BeforeHandlerAsync", op),
                    op => Assert.Equal("middleware2.BeforeHandlerAsync", op),
                    op => Assert.Equal("actionHandler2.HandleAsync", op),
                    op => Assert.Equal("middleware1.AfterHandlerAsync", op),
                    op => Assert.Equal("middleware2.AfterHandlerAsync", op),

                    op => Assert.Equal("middleware1.AfterHandlersAsync", op),
                    op => Assert.Equal("middleware2.AfterHandlersAsync", op)
                );
            }
        }

        public record TestAction : IAction;
        private class QueueActionHandlerMiddleware : IActionHandlerMiddleware
        {
            private readonly string _name;
            private readonly Queue<string> _operationQueue;
            public QueueActionHandlerMiddleware(string name, Queue<string> operationQueue)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                _operationQueue = operationQueue ?? throw new ArgumentNullException(nameof(operationQueue));
            }

            public Task AfterHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterHandlerAsync");
                return Task.CompletedTask;
            }

            public Task AfterHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterHandlersAsync");
                return Task.CompletedTask;
            }

            public Task BeforeHandlerAsync<TAction>(IDispatchContext<TAction> context, IActionHandler<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeHandlerAsync");
                return Task.CompletedTask;
            }

            public Task BeforeHandlersAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionHandler<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeHandlersAsync");
                return Task.CompletedTask;
            }
        }
    }
}
