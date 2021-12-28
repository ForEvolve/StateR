using Bunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using Xunit;

namespace CounterApp;

public class AppTest : TestContext
{
    [Fact]
    public void Should_render_markup()
    {
        var cut = RenderComponent<App>();
        Assert.NotEmpty(cut.Markup);
    }
}
