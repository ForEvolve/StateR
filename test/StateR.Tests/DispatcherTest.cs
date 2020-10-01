using System;
using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StateR
{
    public class DispatcherTest
    {
        private readonly Mock<IDispatchContextFactory> _dispatchContextFactory;
        private readonly Mock<IInterceptorsManager> _interceptorsManager;
        private readonly Mock<IReducersManager> _reducersManager;
        private readonly Mock<IAfterEffectsManager> _afterEffectsManager;
        private readonly Dispatcher sut;

        public DispatcherTest()
        {
            _dispatchContextFactory = new Mock<IDispatchContextFactory>();
            _interceptorsManager = new Mock<IInterceptorsManager>();
            _reducersManager = new Mock<IReducersManager>();
            _afterEffectsManager = new Mock<IAfterEffectsManager>();
            sut = new Dispatcher(_dispatchContextFactory.Object, _interceptorsManager.Object, _reducersManager.Object, _afterEffectsManager.Object);
        }

        public class DispatchAsync : DispatcherTest
        {
            [Fact]
            public async Task Should_create_DispatchContext_using_dispatchContextFactory()
            {
                var action = new TestAction();
                await sut.DispatchAsync(action, CancellationToken.None);
                _dispatchContextFactory.Verify(x => x.Create(action), Times.Once);
            }

            [Fact]
            public async Task Should_send_the_same_DispatchContext_to_all_managers()
            {
                // Arrange
                var action = new TestAction();
                var context = new DispatchContext<TestAction>(action);
                var token = CancellationToken.None;
                _dispatchContextFactory
                    .Setup(x => x.Create(action))
                    .Returns(context);

                // Act
                await sut.DispatchAsync(action, token);

                // Assert
                _interceptorsManager.Verify(x => x.DispatchAsync(context, token), Times.Once);
                _reducersManager.Verify(x => x.DispatchAsync(context, token), Times.Once);
                _afterEffectsManager.Verify(x => x.DispatchAsync(context, token), Times.Once);
            }
            [Fact]
            public async Task Should_call_managers_in_the_expected_order()
            {
                // Arrange
                var action = new TestAction();
                var token = CancellationToken.None;
                var operationQueue = new Queue<string>();
                _interceptorsManager
                    .Setup(x => x.DispatchAsync(It.IsAny< IDispatchContext<TestAction>>(), token))
                    .Callback(() => operationQueue.Enqueue("Interceptors"));
                _reducersManager
                    .Setup(x => x.DispatchAsync(It.IsAny<IDispatchContext<TestAction>>(), token))
                    .Callback(() => operationQueue.Enqueue("Reducers"));
                _afterEffectsManager
                    .Setup(x => x.DispatchAsync(It.IsAny<IDispatchContext<TestAction>>(), token))
                    .Callback(() => operationQueue.Enqueue("AfterEffects"));

                // Act
                await sut.DispatchAsync(action, token);

                // Assert
                Assert.Collection(operationQueue,
                    operation => Assert.Equal("Interceptors", operation),
                    operation => Assert.Equal("Reducers", operation),
                    operation => Assert.Equal("AfterEffects", operation)
                );
            }
        }

        private record TestAction : IAction;
    }
}
