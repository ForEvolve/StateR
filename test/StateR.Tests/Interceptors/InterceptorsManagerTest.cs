using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Interceptors
{
    public class InterceptorsManagerTest
    {
        protected InterceptorsManager CreateInterceptorsManager(Action<ServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            var serviceProvider = services.BuildServiceProvider();
            var middlewares = serviceProvider.GetServices<IInterceptorsMiddleware>();
            return new InterceptorsManager(middlewares, serviceProvider);
        }

        public class DispatchAsync : InterceptorsManagerTest
        {
            [Fact]
            public async Task Should_dispatch_to_all_interceptors()
            {
                // Arrange
                var interceptor1 = new Mock<IActionInterceptor<TestAction>>();
                var interceptor2 = new Mock<IActionInterceptor<TestAction>>();
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                });
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                interceptor1.Verify(x => x.InterceptAsync(context, token), Times.Once);
                interceptor2.Verify(x => x.InterceptAsync(context, token), Times.Once);
            }

            [Fact]
            public async Task Should_call_middleware_and_interceptors_methods_in_order()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var operationQueue = new Queue<string>();
                var interceptor1 = new Mock<IActionInterceptor<TestAction>>();
                interceptor1.Setup(x => x.InterceptAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("interceptor1.InterceptAsync"));
                var interceptor2 = new Mock<IActionInterceptor<TestAction>>();
                interceptor2.Setup(x => x.InterceptAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("interceptor2.InterceptAsync"));
                var middleware1 = new QueueInterceptorsMiddleware("middleware1", operationQueue);
                var middleware2 = new QueueInterceptorsMiddleware("middleware2", operationQueue);
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                    services.AddSingleton<IInterceptorsMiddleware>(middleware1);
                    services.AddSingleton<IInterceptorsMiddleware>(middleware2);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                Assert.Collection(operationQueue,
                    op => Assert.Equal("middleware1.BeforeInterceptorsAsync", op),
                    op => Assert.Equal("middleware2.BeforeInterceptorsAsync", op),

                    op => Assert.Equal("middleware1.BeforeInterceptorAsync", op),
                    op => Assert.Equal("middleware2.BeforeInterceptorAsync", op),
                    op => Assert.Equal("interceptor1.InterceptAsync", op),
                    op => Assert.Equal("middleware1.AfterInterceptorAsync", op),
                    op => Assert.Equal("middleware2.AfterInterceptorAsync", op),

                    op => Assert.Equal("middleware1.BeforeInterceptorAsync", op),
                    op => Assert.Equal("middleware2.BeforeInterceptorAsync", op),
                    op => Assert.Equal("interceptor2.InterceptAsync", op),
                    op => Assert.Equal("middleware1.AfterInterceptorAsync", op),
                    op => Assert.Equal("middleware2.AfterInterceptorAsync", op),

                    op => Assert.Equal("middleware1.AfterInterceptorsAsync", op),
                    op => Assert.Equal("middleware2.AfterInterceptorsAsync", op)
                );
            }

            [Fact]
            public async Task Should_break_interception_when_StopInterception_is_true()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var interceptor1 = new Mock<IActionInterceptor<TestAction>>();
                interceptor1.Setup(x => x.InterceptAsync(context, token))
                    .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken) => context.StopInterception = true);
                var interceptor2 = new Mock<IActionInterceptor<TestAction>>();
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                interceptor1.Verify(x => x.InterceptAsync(context, token), Times.Once);
                interceptor2.Verify(x => x.InterceptAsync(context, token), Times.Never);
            }

        }

        public record TestAction : IAction;

        private class QueueInterceptorsMiddleware : IInterceptorsMiddleware
        {
            private readonly string _name;
            private readonly Queue<string> _operationQueue;
            public QueueInterceptorsMiddleware(string name, Queue<string> operationQueue)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                _operationQueue = operationQueue ?? throw new ArgumentNullException(nameof(operationQueue));
            }

            public Task AfterInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterInterceptorAsync");
                return Task.CompletedTask;
            }

            public Task AfterInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterInterceptorsAsync");
                return Task.CompletedTask;
            }

            public Task BeforeInterceptorAsync<TAction>(IDispatchContext<TAction> context, IActionInterceptor<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeInterceptorAsync");
                return Task.CompletedTask;
            }

            public Task BeforeInterceptorsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionInterceptor<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeInterceptorsAsync");
                return Task.CompletedTask;
            }
        }
    }
}
