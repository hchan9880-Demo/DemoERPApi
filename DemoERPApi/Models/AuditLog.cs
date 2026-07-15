using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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


    [Table("AuditLogs")]
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
        [MaxLength(100)]
        public string EntityName { get; set; } = string.Empty;



        /*
        Identifier of the affected entity.

        Example:
            CRMCustomerID = CRM100
        */
        [MaxLength(400)]
        public string EntityId { get; set; } = string.Empty;



        /*
        Type of database operation.

        Expected values:
            CREATE
            UPDATE
            DELETE
            RESTORE
        */
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;



        /*
        JSON snapshot before modification.
        */
        public string? OldValues { get; set; }



        /*
        JSON snapshot after modification.
        */
        public string? NewValues { get; set; }



        /*
        User responsible for the change.

        Examples:
            admin
            henry@demoerp.com
            system
        */
        [MaxLength(100)]
        public string ChangedBy { get; set; } = string.Empty;



        /*
        API request correlation identifier.

        Used for:
            - Request tracing
            - Troubleshooting
            - Correlating API logs
        */
        [MaxLength(800)]
        public string? RequestId { get; set; }



        /*
        Date and time when the change occurred.
        */
        public DateTime ChangedDate { get; set; }

    }
}