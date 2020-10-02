using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.AfterEffects
{
    public class AfterEffectsManagerTest
    {
        protected AfterEffectsManager CreateAfterEffectsManager(Action<ServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            var serviceProvider = services.BuildServiceProvider();
            var middlewares = serviceProvider.GetServices<IAfterEffectsMiddleware>();
            return new AfterEffectsManager(middlewares, serviceProvider);
        }

        public class DispatchAsync : AfterEffectsManagerTest
        {
            [Fact]
            public async Task Should_handle_all_after_effects()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var afterEffect1 = new Mock<IActionAfterEffects<TestAction>>();
                var afterEffect2 = new Mock<IActionAfterEffects<TestAction>>();
                var sut = CreateAfterEffectsManager(services =>
                {
                    services.AddSingleton(afterEffect1.Object);
                    services.AddSingleton(afterEffect2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                afterEffect1.Verify(x => x.HandleAfterEffectAsync(context, token), Times.Once);
                afterEffect2.Verify(x => x.HandleAfterEffectAsync(context, token), Times.Once);
            }

            [Fact]
            public async Task Should_break_after_effects_when_StopAfterEffect_is_true()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var afterEffect1 = new Mock<IActionAfterEffects<TestAction>>();
                afterEffect1.Setup(x => x.HandleAfterEffectAsync(context, token))
                    .Callback((IDispatchContext<TestAction> context, CancellationToken cancellationToken) => context.StopAfterEffect = true);
                var afterEffect2 = new Mock<IActionAfterEffects<TestAction>>();
                var sut = CreateAfterEffectsManager(services =>
                {
                    services.AddSingleton(afterEffect1.Object);
                    services.AddSingleton(afterEffect2.Object);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                afterEffect1.Verify(x => x.HandleAfterEffectAsync(context, token), Times.Once);
                afterEffect2.Verify(x => x.HandleAfterEffectAsync(context, token), Times.Never);
            }

            [Fact]
            public async Task Should_call_middleware_and_after_effects_methods_in_order()
            {
                // Arrange
                var context = new DispatchContext<TestAction>(new TestAction());
                var token = CancellationToken.None;

                var operationQueue = new Queue<string>();
                var afterEffect1 = new Mock<IActionAfterEffects<TestAction>>();
                afterEffect1.Setup(x => x.HandleAfterEffectAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("afterEffect1.HandleAfterEffectAsync"));
                var afterEffect2 = new Mock<IActionAfterEffects<TestAction>>();
                afterEffect2.Setup(x => x.HandleAfterEffectAsync(context, token))
                    .Callback(() => operationQueue.Enqueue("afterEffect2.HandleAfterEffectAsync"));
                var middleware1 = new QueueAfterEffectsMiddleware("middleware1", operationQueue);
                var middleware2 = new QueueAfterEffectsMiddleware("middleware2", operationQueue);
                var sut = CreateAfterEffectsManager(services =>
                {
                    services.AddSingleton(afterEffect1.Object);
                    services.AddSingleton(afterEffect2.Object);
                    services.AddSingleton<IAfterEffectsMiddleware>(middleware1);
                    services.AddSingleton<IAfterEffectsMiddleware>(middleware2);
                });

                // Act
                await sut.DispatchAsync(context, token);

                // Assert
                Assert.Collection(operationQueue,
                    op => Assert.Equal("middleware1.BeforeAfterEffectsAsync", op),
                    op => Assert.Equal("middleware2.BeforeAfterEffectsAsync", op),

                    op => Assert.Equal("middleware1.BeforeAfterEffectAsync", op),
                    op => Assert.Equal("middleware2.BeforeAfterEffectAsync", op),
                    op => Assert.Equal("afterEffect1.HandleAfterEffectAsync", op),
                    op => Assert.Equal("middleware1.AfterAfterEffectAsync", op),
                    op => Assert.Equal("middleware2.AfterAfterEffectAsync", op),

                    op => Assert.Equal("middleware1.BeforeAfterEffectAsync", op),
                    op => Assert.Equal("middleware2.BeforeAfterEffectAsync", op),
                    op => Assert.Equal("afterEffect2.HandleAfterEffectAsync", op),
                    op => Assert.Equal("middleware1.AfterAfterEffectAsync", op),
                    op => Assert.Equal("middleware2.AfterAfterEffectAsync", op),

                    op => Assert.Equal("middleware1.AfterAfterEffectsAsync", op),
                    op => Assert.Equal("middleware2.AfterAfterEffectsAsync", op)
                );
            }
        }

        public record TestAction : IAction;

        private class QueueAfterEffectsMiddleware : IAfterEffectsMiddleware
        {
            private readonly string _name;
            private readonly Queue<string> _operationQueue;
            public QueueAfterEffectsMiddleware(string name, Queue<string> operationQueue)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                _operationQueue = operationQueue ?? throw new ArgumentNullException(nameof(operationQueue));
            }

            public Task AfterAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IActionAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterAfterEffectAsync");
                return Task.CompletedTask;
            }

            public Task AfterAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.AfterAfterEffectsAsync");
                return Task.CompletedTask;
            }

            public Task BeforeAfterEffectAsync<TAction>(IDispatchContext<TAction> context, IActionAfterEffects<TAction> interceptor, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeAfterEffectAsync");
                return Task.CompletedTask;
            }

            public Task BeforeAfterEffectsAsync<TAction>(IDispatchContext<TAction> context, IEnumerable<IActionAfterEffects<TAction>> interceptors, CancellationToken cancellationToken) where TAction : IAction
            {
                _operationQueue.Enqueue($"{_name}.BeforeAfterEffectsAsync");
                return Task.CompletedTask;
            }
        }
    }
}
