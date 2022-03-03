using Microsoft.Extensions.DependencyInjection;
using System;
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

        [Theory]
        [InlineData(typeof(TestState1), typeof(InitialTestState2))]
        [InlineData(typeof(TestState1), typeof(NotAState))]
        public void Should_throw_an_InvalidInitialStateException_when_the_initialState_is_invalid(Type stateType, Type initialStateType)
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);

            // Act & Assert
            var ex = Assert.Throws<InvalidInitialStateException>(() => sut.AddState(stateType, initialStateType));
            Assert.Same(initialStateType, ex.InitialStateType);
        }
    }

    public class AddAction_TAction_TState
    {
        [Fact]
        public void Should_add_TAction_to_Actions()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);

            // Act
            sut.AddAction<TestAction1, TestState1>();

            // Assert
            Assert.Collection(sut.Actions,
                type => Assert.Equal(typeof(TestAction1), type)
            );
        }
    }

    public class AddAction_Type
    {
        [Fact]
        public void Should_add_TAction_to_Actions()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);
            var actionType = typeof(TestAction1);

            // Act
            sut.AddAction(actionType);

            // Assert
            Assert.Collection(sut.Actions,
                type => Assert.Same(actionType, type)
            );
        }

        [Theory]
        [InlineData(typeof(NotAnAction))]
        public void Should_throw_an_InvalidActionException_when_actionType_is_invalid(Type actionType)
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services);

            // Act & Assert
            var ex = Assert.Throws<InvalidActionException>(() => sut.AddAction(actionType));
            Assert.Same(actionType, ex.ActionType);
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
