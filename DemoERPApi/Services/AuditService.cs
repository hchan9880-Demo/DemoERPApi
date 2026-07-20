/*
===============================================================================

     Implements audit logging business logic.

 Responsibilities:

     1. Serialize old entity values
     2. Serialize new entity values
     3. Create AuditLog record
     4. Save audit information into database

 Audit records provide:
     - Data traceability
     - Compliance support
     - Change investigation
     - User accountability

CustomerController

       |
       |
       v

IAuditService

       |
       |
       v

AuditService

       |
       |
       v

AuditLogs Table
===============================================================================
*/



/// Audit service for logging CREATE, UPDATE, DELETE operations.

using DemoERPApi.Data;
using DemoERPApi.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DemoERPApi.Interfaces;


namespace DemoERPApi.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _context;


        public AuditService(AppDbContext context)
        {
            _context = context;
        }



        /*
 ===============================================================================
 Log Create Operation

 Creates an audit record when a new entity is created.

 Example:

 Entity:
     Customer

 Action:
     CREATE

 OldValues:
     null

 NewValues:
     New entity snapshot JSON

 ===============================================================================
 */


        
        /// Logs a CREATE operation.
        
        public async Task LogCreateAsync<T>(
            string entityName,
            string entityId,
            T newValues,
            string changedBy,
            string? requestId)
        {
            await LogAsync(
                entityName,
                entityId,
                "CREATE",
                null,
                newValues,
                changedBy,
                requestId);
        }




        /*
        ===========================================================================
        Log Update Operation

        Example:
            Customer phone number changed

        OldValues:
            Previous customer data

        NewValues:
            Updated customer data
        ===========================================================================
        */

        
        /// Logs an UPDATE operation with old and new values.
        
        public async Task LogUpdateAsync<T>(
            string entityName,
            string entityId,
            T oldEntity,
            T newEntity,
            string changedBy,
            string? requestId)
        {
            await LogAsync(
                entityName,
                entityId,
                "UPDATE",
                oldEntity,
                newEntity,
                changedBy,
                requestId);
        }




        /*
        ===========================================================================
        Log Delete Operation

        Example:
            Customer deleted

        OldValues:
            Deleted customer snapshot

        NewValues:
            null
        ===========================================================================
        */

        
        /// Logs a DELETE operation.
        
        public async Task LogDeleteAsync<T>(
            string entityName,
            string entityId,
            T deletedEntity,
            string changedBy,
            string? requestId)
        {
            await LogAsync(
                entityName,
                entityId,
                "DELETE",
                deletedEntity,
                null,
                changedBy,
                requestId);
        }





        /*
        ===========================================================================
        Generic Audit Logger

        Processing Flow:

            Entity Object
                  |
                  v
            JSON Serialization
                  |
                  v
            Create AuditLog
                  |
                  v
            Save Database

        ===========================================================================
        */

        
        /// Generic audit logger with JSON serialization.
        

        public async Task LogAsync(
            string entityName,
            string entityId,
            string action,
            object? oldValues,
            object? newValues,
            string changedBy,
            string? requestId)
        {
            var auditLogs = new AuditLogs
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues == null ? null : JsonSerializer.Serialize(oldValues),
                NewValues = newValues == null ? null : JsonSerializer.Serialize(newValues),
                ChangedBy = changedBy,
                RequestId = requestId,
                ChangedDate = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLogs);
            await _context.SaveChangesAsync();
        }

    }
}