using DemoERPApi.Tests.Fixtures;

namespace DemoERPApi.Tests;

public static class TestApplication
{
    private static readonly Lazy<CustomWebApplicationFactory> _factory =
        new(() => new CustomWebApplicationFactory());


    public static CustomWebApplicationFactory Factory
    {
        get
        {
            return _factory.Value;
        }
    }
}