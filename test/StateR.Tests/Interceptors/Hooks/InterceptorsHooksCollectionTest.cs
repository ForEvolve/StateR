using Castle.DynamicProxy;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Interceptors.Hooks
{
    public class InterceptorsHooksCollectionTest
    {
        private readonly Mock<IBeforeInterceptorHook> _before1Mock = new();
        private readonly Mock<IBeforeInterceptorHook> _before2Mock = new();
        private readonly Mock<IAfterInterceptorHook> _after1Mock = new();
        private readonly Mock<IAfterInterceptorHook> _after2Mock = new();

        private readonly Mock<IInterceptor<TestAction>> _afterEffectMock = new();
        private readonly IDispatchContext<TestAction> dispatchContext = new DispatchContext<TestAction>(new TestAction(), new Mock<IDispatcher>().Object);
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        private readonly InterceptorsHooksCollection sut;

        public InterceptorsHooksCollectionTest()
        {
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
                await sut.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken);

                // Assert
                _before1Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
                _before2Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
                _after1Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
                _after2Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
            }
        }
        public class AfterHandlerAsync : InterceptorsHooksCollectionTest
        {
            [Fact]
            public async Task Should_call_all_hooks()
            {
                // Act
                await sut.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken);

                // Assert
                _before1Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
                _before2Mock.Verify(x => x.BeforeHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Never);
                _after1Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
                _after2Mock.Verify(x => x.AfterHandlerAsync(dispatchContext, _afterEffectMock.Object, _cancellationToken), Times.Once);
            }
        }
        public record TestAction : IAction;
    }
}
