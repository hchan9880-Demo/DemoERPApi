using Microsoft.AspNetCore.Mvc.Testing;

namespace DemoERPApi.Tests.TestHelpers;

public class CustomWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
}