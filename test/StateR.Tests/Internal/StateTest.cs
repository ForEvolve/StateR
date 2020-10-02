using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Internal
{
    public class StateTest
    {
        private readonly TestState _initialState = new(0);
        private readonly Mock<IInitialState<TestState>> _initialMock;
        private readonly State<TestState> sut;

        public StateTest()
        {
            _initialMock = new();
            _initialMock.Setup(x => x.Value).Returns(_initialState);
            sut = new(_initialMock.Object);
        }

        public class Ctor : StateTest
        {
            [Fact]
            public void Should_set_Current_with_initial_state()
            {
                Assert.Same(_initialState, sut.Current);
            }
        }

        public class Set : StateTest
        {
            [Fact]
            public void Should_set_Current()
            {
                var newState = new TestState(1);
                sut.Set(newState);
                Assert.Same(newState, sut.Current);
            }

            [Fact]
            public void Should_not_set_same_state()
            {
                var newState = new TestState(0);
                sut.Set(newState);
                Assert.NotSame(newState, sut.Current);
                Assert.Same(_initialState, sut.Current);
            }
        }

        public class Notify : StateTest
        {
            [Fact]
            public void Should_notify_subscribers()
            {
                // Arrange
                var subscriberQueue = new Queue<string>();
                Action sub1 = () => subscriberQueue.Enqueue("Sub1");
                Action sub2 = () => subscriberQueue.Enqueue("Sub2");
                Action sub3 = () => subscriberQueue.Enqueue("Sub3");
                sut.Subscribe(sub1);
                sut.Subscribe(sub2);
                sut.Subscribe(sub3);
                sut.Unsubscribe(sub2);

                // Act
                sut.Notify();

                // Assert
                Assert.Collection(subscriberQueue,
                    op => Assert.Equal("Sub1", op),
                    op => Assert.Equal("Sub3", op)
                );
            }
        }

        public record TestState(int Value) : StateBase;
    }
}
