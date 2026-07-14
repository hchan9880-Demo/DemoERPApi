using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DemoERPApi.Controllers;


[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{

    // =====================================================
    // Used only for integration testing
    //
    // Purpose:
    // Force an unhandled exception so that
    // ExceptionMiddleware can be verified.
    // Expected result:
    // HTTP 500
    // =====================================================

    [AllowAnonymous]
    [HttpGet("throw")]
    public IActionResult ThrowException()
    {
        throw new Exception(
            "Test exception for middleware validation");
    }
}