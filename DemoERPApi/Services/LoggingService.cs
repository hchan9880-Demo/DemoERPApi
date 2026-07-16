using DemoERPApi.Data;
using DemoERPApi.Models;
/*
 * Logging Flow
 * 
Client

↓

REST API

↓

Controller

↓

Service

↓

Repository

↓

ILogger

↓

Logging Service

↓

SyncLog Table

↓

SQL Server
 
 */
namespace DemoERPApi.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly AppDbContext _db;


        public LoggingService(AppDbContext db)
        {
            _db = db;
        }

        /*
        public async Task LogAsync(
            string crmCustomerId,
            string operation,
            string status,
            string message,
            string? username,
            string? requestId,
            int executionTimeMs)
        {

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
        }
    }
*/

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

                Console.WriteLine("SyncLog saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save SyncLog:");
                Console.WriteLine(ex.ToString());
                throw;
            }
        }


        }
    }
