using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StateR.Internal;

public class StatorBuilderTest
{
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
