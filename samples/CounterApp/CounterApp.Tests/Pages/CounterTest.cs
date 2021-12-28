using Bunit;
using Xunit;

namespace CounterApp.Pages;

public class CounterTest : TestContext
{
    public CounterTest()
    {
        ProgramExtensions.RegisterServices(Services);
    }

    [Fact]
    public void H1_should_render_Counter()
    {
        var cut = RenderComponent<Counter>();
        var h1 = cut.Find("h1");
        Assert.Equal("Counter", h1.TextContent);
    }

    [Fact]
    public void Counter_text_should_be_zero()
    {
        var cut = RenderComponent<Counter>();
        var p = cut.Find("p");
        Assert.Equal("Current count: 0", p.TextContent);
    }

    [Fact]
    public void Increment_button_should_increment_count_by_1()
    {
        var cut = RenderComponent<Counter>();
        var button = cut.FindAll("button").First();
        var p = cut.Find("p");

        button.Click();

        Assert.Equal("Current count: 1", p.TextContent);
    }

    [Fact]
    public void Decrement_button_should_decrement_count_by_1()
    {
        var cut = RenderComponent<Counter>();
        var button = cut.FindAll("button").Last();
        var p = cut.Find("p");

        button.Click();

        Assert.Equal("Current count: -1", p.TextContent);
    }
}