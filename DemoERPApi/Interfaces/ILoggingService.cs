/*
 CustomerController
        |
        v
ILogger
        |
        v
LoggingService
        |
        v
SyncLog Table




Exception Handling Flow
Client Request

↓

Controller

↓

Service

↓

Repository

↓

Unhandled Exception

↓

Exception Middleware

↓

ILogger

↓

SyncLog

↓

HTTP 500 ProblemDetails Response
 
 */
namespace DemoERPApi.Interfaces
{
    public interface ILoggingService
    {
        Task LogAsync(
            string crmCustomerId,
            string operation,
            string status,
            string message,
            string? username,
            string? requestId,
            int executionTimeMs);
    }

}
