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
using DemoERPApi.Models;

namespace DemoERPApi.Interfaces
{
    public interface IAuditService
    {
        /*
        Logs a new entity creation event.

        Example:
            Customer created successfully
        */
        Task LogCreateAsync<T>(
            string entityName,
            string entityId,
            T newEntity,
            string changedBy,
            string? requestId);



        /*
        Logs an entity update event.

        Stores:
            OldValues
            NewValues
        */
        Task LogUpdateAsync<T>(
            string entityName,
            string entityId,
            T oldEntity,
            T newEntity,
            string changedBy,
            string? requestId);



        /*
        Logs an entity deletion event.

        Stores:
            Deleted entity snapshot
        */
        Task LogDeleteAsync<T>(
            string entityName,
            string entityId,
            T deletedEntity,
            string changedBy,
            string? requestId);



        /*
        Generic audit logging method.

        Used internally by:
            LogCreateAsync
            LogUpdateAsync
            LogDeleteAsync

        Supports future audit actions.
        */
        Task LogAsync(
            string entityName,
            string entityId,
            string action,
            object? oldValues,
            object? newValues,
            string changedBy,
            string? requestId);
    }
}