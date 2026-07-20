/*
===============================================================================

 Purpose:
     Provides a centralized interface for creating audit trail records.

 Responsibilities:
     - Capture entity changes
     - Record user activity
     - Track API request correlation
     - Persist audit information

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

using System.Threading.Tasks;

namespace DemoERPApi.Interfaces;


/// Interface for audit logging of CREATE, UPDATE, DELETE operations.

public interface IAuditService
{
    
    /// Logs a CREATE operation.
    
    Task LogCreateAsync<T>(
        string entityName,
        string entityId,
        T newEntity,
        string changedBy,
        string? requestId);

    
    /// Logs an UPDATE operation with old and new values.
    
    Task LogUpdateAsync<T>(
        string entityName,
        string entityId,
        T oldEntity,
        T newEntity,
        string changedBy,
        string? requestId);

    
    /// Logs a DELETE operation.
    
    Task LogDeleteAsync<T>(
        string entityName,
        string entityId,
        T deletedEntity,
        string changedBy,
        string? requestId);

    
    /// Generic audit logger with JSON serialization of old/new values.
    
    Task LogAsync(
        string entityName,
        string entityId,
        string action,
        object? oldValues,
        object? newValues,
        string changedBy,
        string? requestId);
}