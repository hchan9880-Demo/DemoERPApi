using DemoERPApi.Data;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;

namespace DemoERPApi.Services;


/// Service responsible for logging synchronization operations to the database.
/// Handles structured logging of sync events with error handling.

public class LoggingService : ILoggingService
{
    private readonly AppDbContext _db;

    
    /// Initializes a new instance of the <see cref="LoggingService"/> class.
    
    /// <param name="db">Database context for persisting logs</param>
    public LoggingService(AppDbContext db)
    {
        _db = db;
    }

    
    /// Asynchronously logs a synchronization operation.
    
    /// <param name="crmCustomerId">Customer CRM identifier</param>
    /// <param name="operation">Operation type (e.g., SYNC, CREATE, UPDATE)</param>
    /// <param name="status">Operation status (e.g., SUCCESS, FAILED)</param>
    /// <param name="message">Log message with operation details</param>
    /// <param name="username">User who performed the operation (optional)</param>
    /// <param name="requestId">Request correlation ID (optional)</param>
    /// <param name="executionTimeMs">Operation execution time in milliseconds</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task LogAsync(
        string crmCustomerId,
        string operation,
        string status,
        string message,
        string? username,
        string? requestId,
        int executionTimeMs)
    {
        try
        {
            // Create and persist sync log entry
            _db.SyncLogs.Add(new SyncLogs
            {
                CRMCustomerID = crmCustomerId,
                Operation = operation,
                Status = status,
                Message = message,
                Username = username,
                RequestId = requestId,
                CreatedDate = DateTime.UtcNow,
                ExecutionTimeMs = executionTimeMs
            });

            await _db.SaveChangesAsync();

            // Debug: Confirm successful save
            Console.WriteLine("SyncLog saved successfully.");
        }
        catch (Exception ex)
        {
            // Log error and rethrow to maintain audit trail
            Console.WriteLine("Failed to save SyncLog:");
            Console.WriteLine(ex.ToString());
            throw;
        }
    }
}