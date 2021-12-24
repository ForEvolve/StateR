using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using StateR.Interceptors.Hooks;
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
        private readonly Mock<IInterceptorsHooksCollection> _hooksCollectionMock = new();
        protected InterceptorsManager CreateInterceptorsManager(Action<ServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            services.TryAddSingleton(_hooksCollectionMock.Object);
            var serviceProvider = services.BuildServiceProvider();
            return new InterceptorsManager(_hooksCollectionMock.Object, serviceProvider);
        }

        public class DispatchAsync : InterceptorsManagerTest
        {
            [Fact]
            public async Task Should_dispatch_to_all_interceptors()
            {
                // Arrange
                var cancellationTokenSource = new CancellationTokenSource();
                var interceptor1 = new Mock<IInterceptor<TestAction>>();
                var interceptor2 = new Mock<IInterceptor<TestAction>>();
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                });
                var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

                // Act
                await sut.DispatchAsync(context);

                // Assert
                interceptor1.Verify(x => x.InterceptAsync(context, cancellationTokenSource.Token), Times.Once);
                interceptor2.Verify(x => x.InterceptAsync(context, cancellationTokenSource.Token), Times.Once);
            }

            [Fact]
            public async Task Should_call_middleware_and_interceptors_methods_in_order()
            {
                // Arrange
                var cancellationTokenSource = new CancellationTokenSource();
                var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

                var operationQueue = new Queue<string>();
                var interceptor1 = new Mock<IInterceptor<TestAction>>();
                interceptor1.Setup(x => x.InterceptAsync(context, cancellationTokenSource.Token))
                    .Callback(() => operationQueue.Enqueue("interceptor1.InterceptAsync"));
                var interceptor2 = new Mock<IInterceptor<TestAction>>();
                interceptor2.Setup(x => x.InterceptAsync(context, cancellationTokenSource.Token))
                    .Callback(() => operationQueue.Enqueue("interceptor2.InterceptAsync"));
                _hooksCollectionMock
                    .Setup(x => x.BeforeHandlerAsync(context, It.IsAny<IInterceptor<TestAction>>(), cancellationTokenSource.Token))
                    .Callback(() => operationQueue.Enqueue("BeforeHandlerAsync"));
                _hooksCollectionMock
                    .Setup(x => x.AfterHandlerAsync(context, It.IsAny<IInterceptor<TestAction>>(), cancellationTokenSource.Token))
                    .Callback(() => operationQueue.Enqueue("AfterHandlerAsync"));
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                });

                // Act
                await sut.DispatchAsync(context);

                // Assert
                Assert.Collection(operationQueue,
                    op => Assert.Equal("BeforeHandlerAsync", op),
                    op => Assert.Equal("interceptor1.InterceptAsync", op),
                    op => Assert.Equal("AfterHandlerAsync", op),

                    op => Assert.Equal("BeforeHandlerAsync", op),
                    op => Assert.Equal("interceptor2.InterceptAsync", op),
                    op => Assert.Equal("AfterHandlerAsync", op)
                );
            }

            [Fact]
            public async Task Should_break_interception_when_Cancel()
            {
                // Arrange
                var cancellationTokenSource = new CancellationTokenSource();
                var context = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object, cancellationTokenSource);

                var interceptor1 = new Mock<IInterceptor<TestAction>>();
                interceptor1.Setup(x => x.InterceptAsync(context, cancellationTokenSource.Token))
                    .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken) => context.Cancel());
                var interceptor2 = new Mock<IInterceptor<TestAction>>();
                var sut = CreateInterceptorsManager(services =>
                {
                    services.AddSingleton(interceptor1.Object);
                    services.AddSingleton(interceptor2.Object);
                });

                // Act
                await Assert.ThrowsAsync<OperationCanceledException>(()
                    => sut.DispatchAsync(context));

                // Assert
                interceptor1.Verify(x => x.InterceptAsync(context, cancellationTokenSource.Token), Times.Once);
                interceptor2.Verify(x => x.InterceptAsync(context, cancellationTokenSource.Token), Times.Never);
            }
        }

        public record TestAction : IAction;
    }
}
