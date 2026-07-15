using System.ComponentModel.DataAnnotations;

namespace DemoERPApi.Models
{
    /*
    ===============================================================================
    
     Purpose:
         Stores historical records of data changes made within the system.

     AuditLogs tracks:
         - What entity was changed
         - Which record was affected
         - What operation occurred
         - Previous and new values
         - Who performed the change
         - Which API request triggered the change
         - When the change occurred

     Difference from SyncLogs:

         AuditLogs:
             Tracks DATA changes.

         SyncLogs:
             Tracks APPLICATION execution events.


    ===============================================================================
    */

    public class AuditLog
    {

        /*
        Unique identifier for audit record.

        Database:
            AuditId INT IDENTITY PRIMARY KEY
        */
        [Key]
        public int AuditId { get; set; }



        /*
        Name of the entity being changed.

        Examples:
            Customer
            Order
            Invoice
        */
        public string EntityName { get; set; } = string.Empty;



        /*
        Identifier of the affected entity.

        Example:
            CRMCustomerID = CRM100
        */
        public string EntityId { get; set; } = string.Empty;



        /*
        Type of database operation.

        Expected values:
            CREATE
            UPDATE
            DELETE
            RESTORE
        */
        public string Action { get; set; } = string.Empty;



        /*
        JSON snapshot of data before modification.

        Example:
        {
            "Phone": "604-111-1111",
            "Email": "old@email.com"
        }

        Used for:
            - Change comparison
            - Data recovery investigation
        */
        public string? OldValues { get; set; }



        /*
        JSON snapshot of data after modification.

        Example:
        {
            "Phone": "604-222-2222",
            "Email": "new@email.com"
        }
        */
        public string? NewValues { get; set; }



        /*
        User or service account responsible for the change.

        Examples:
            admin
            henry@demoerp.com
            system
        */
        public string ChangedBy { get; set; } = string.Empty;



        /*
        Unique API request identifier.

        Used for:
            - Request tracing
            - Troubleshooting
            - Correlating API logs
        */
        public string? RequestId { get; set; }



        /*
        Date and time when the change occurred.
        */
        public DateTime ChangedDate { get; set; }

    }
}