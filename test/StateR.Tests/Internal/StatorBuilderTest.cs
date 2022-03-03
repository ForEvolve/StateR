using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace StateR.Internal;

public class StatorBuilderTest
{
    public class AddState_TState : StatorBuilderTest
    {
        [Fact]
        public void Should_add_TState_to_States_and_TInitialState_to_InitialStates()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);

            // Act
            sut.AddState<TestState1, InitialTestState1>();

            // Assert
            Assert.Collection(sut.States,
                type => Assert.Equal(typeof(TestState1), type)
            );
            Assert.Collection(sut.InitialStates,
                type => Assert.Equal(typeof(InitialTestState1), type)
            );
        }
    }
    public class AddState_Type : StatorBuilderTest
    {
        [Fact]
        public void Should_add_a_valid_state_type_to_States()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);

            // Act
            sut.AddState(typeof(TestState1), typeof(InitialTestState1));

            // Assert
            Assert.Collection(sut.States,
                type => Assert.Equal(typeof(TestState1), type)
            );
        }

        [Fact]
        public void Should_throw_an_InvalidStateException_when_the_state_type_does_not_inherit_StateBase()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);
            var stateType = typeof(NotAState);
            var initialStateType = typeof(InitialTestState1);

            // Act & Assert
            var ex = Assert.Throws<InvalidStateException>(() => sut.AddState(stateType, initialStateType));
            Assert.Same(stateType, ex.StateType);
        }

        [Fact]
        public void Should_throw_an_InvalidInitialStateException_when_the_initialState_does_not_implement_IInitialState()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);
            var stateType = typeof(TestState1);
            var initialStateType = typeof(NotAState);

            // Act & Assert
            var ex = Assert.Throws<InvalidInitialStateException>(() => sut.AddState(stateType, initialStateType));
            Assert.Same(initialStateType, ex.InitialStateType);
        }

        [Fact]
        public void Should_throw_an_InvalidInitialStateException_when_the_initialState_does_not_initialize_state()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);
            var stateType = typeof(TestState1);
            var initialStateType = typeof(InitialTestState2);

            // Act & Assert
            var ex = Assert.Throws<InvalidInitialStateException>(() => sut.AddState(stateType, initialStateType));
            Assert.Same(initialStateType, ex.InitialStateType);
        }
    }

    public class AddTypes : StatorBuilderTest
    {
        [Fact]
        public void Should_add_disctict_tally_types()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);
            var types = new Type[]
            {
                    typeof(StatorBuilderTest),
                    typeof(StatorBuilderTest),
            };
            var types2 = new Type[]
            {
                    typeof(AddTypes),
                    typeof(StatorBuilderTest),
            };

            // Act
            sut.AddTypes(types);
            sut.AddTypes(types2);

            // Assert
            Assert.Collection(sut.All,
                type => Assert.Equal(typeof(StatorBuilderTest), type),
                type => Assert.Equal(typeof(AddTypes), type)
            );
        }
    }
}
