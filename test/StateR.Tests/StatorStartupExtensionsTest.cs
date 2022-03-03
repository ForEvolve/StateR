using Microsoft.Extensions.DependencyInjection;
using StateR.Internal;
using StateR.Pipeline;
using StateR.Updaters;
using System;
using Xunit;
namespace StateR;

public class StatorStartupExtensionsTest
{
    public class AddStateR : StatorStartupExtensionsTest
    {
        [Fact(Skip = "TODO: implement tests")]
        public void Should_be_tested()
        {
            // Arrange


            // Act


            // Assert
            throw new NotImplementedException();
        }
    }

    public class Apply : StatorStartupExtensionsTest
    {
        [Fact]
        public void Should_add_IState_to_the_ServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services)
                .AddState<TestState1, InitialTestState1>()
                .AddState<TestState2, InitialTestState2>()
                .AddState<TestState3, InitialTestState3>()
            ;

            // Act
            sut.Apply();

            // Assert
            var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IState<TestState1>>();
            sp.GetRequiredService<IState<TestState2>>();
            sp.GetRequiredService<IState<TestState3>>();
        }

        [Fact]
        public void Should_add_IUpdater_and_IActionFilter_to_the_ServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var sut = new StatorBuilder(services)
                .AddState<TestState1, InitialTestState1>()
                .AddAction<TestAction1, TestState1>()
                .AddUpdater<TestUpdaters, TestAction1, TestState1>()
            ;

            // Act
            sut.Apply();

            // Assert
            var sp = services.BuildServiceProvider();
            sp.GetRequiredService<IUpdater<TestAction1, TestState1>>();
            sp.GetRequiredService<IActionFilter<TestAction1, TestState1>>();
        }
    }
}
