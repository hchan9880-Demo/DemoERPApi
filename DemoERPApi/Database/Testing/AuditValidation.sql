/*
===============================================================================
 DemoERPApi
 Script: AuditValidation.sql

 Purpose
 -------
 Validation and reporting queries for the AuditLogs table.

 These queries validate:
   1. Audit record creation
   2. Entity change history
   3. Update tracking
   4. JSON before/after value comparison
   5. User activity tracking

 Used for:
   - Database validation testing
   - Audit trail verification
   - Data integrity checks
   - Production support troubleshooting

===============================================================================
*/


/*
===============================================================================
 1. View Latest Audit Activity

 Purpose:
   Confirm that audit records are being created successfully
   and review the most recent system changes.
===============================================================================
*/

SELECT *
FROM dbo.AuditLogs
ORDER BY ChangedDate DESC;
GO



/*
===============================================================================
 2. View Customer Entity Audit History

 Purpose:
   Retrieve all audit records related to Customer changes.

 Validation:
   - Customer creation
   - Customer updates
   - Customer deletions
===============================================================================
*/

SELECT *
FROM dbo.AuditLogs
WHERE EntityName = 'Customer'
ORDER BY ChangedDate DESC;
GO



/*
===============================================================================
 3. View Specific Customer Change History

 Purpose:
   Review the complete audit timeline for a specific customer.

 Example:
   Customer ID = CUST1001
===============================================================================
*/

SELECT *
FROM dbo.AuditLogs
WHERE EntityId = 'CUST1001'
ORDER BY ChangedDate DESC;
GO



/*
===============================================================================
 4. View Update Operations Only

 Purpose:
   Validate that UPDATE operations are being captured correctly.

 Common Use Cases:
   - Verify field-level changes
   - Investigate unexpected data modifications
===============================================================================
*/

SELECT *
FROM dbo.AuditLogs
WHERE Action = 'UPDATE'
ORDER BY ChangedDate DESC;
GO



/*
===============================================================================
 5. Compare Previous and New Values from JSON Data

 Purpose:
   Extract changed values stored in OldValues and NewValues JSON columns.

 Example:
   Compare customer's previous phone number
   with the updated phone number.
===============================================================================
*/

SELECT
    EntityId,
    JSON_VALUE(OldValues, '$.Phone') AS OldPhone,
    JSON_VALUE(NewValues, '$.Phone') AS NewPhone,
    ChangedBy,
    ChangedDate
FROM dbo.AuditLogs
WHERE Action = 'UPDATE'
ORDER BY ChangedDate DESC;
GO



/*
===============================================================================
 6. Audit Activity by User

 Purpose:
   Identify users or services making database changes.

 Used For:
   - Security reviews
   - User activity monitoring
   - Compliance reporting
===============================================================================
*/

SELECT
    ChangedBy,
    COUNT(*) AS ChangeCount
FROM dbo.AuditLogs
GROUP BY ChangedBy
ORDER BY ChangeCount DESC;
GO



/*
===============================================================================
 7. Audit Action Summary

 Purpose:
   Display the number of audit records by operation type.

 Example Results:
   INSERT  - New records created
   UPDATE  - Existing records modified
   DELETE  - Records removed
===============================================================================
*/

SELECT
    Action,
    COUNT(*) AS TotalChanges
FROM dbo.AuditLogs
GROUP BY Action
ORDER BY TotalChanges DESC;
GO



/*
===============================================================================
 8. Validate Audit Record Structure

 Purpose:
   Review key audit fields required for traceability.

 Expected Fields:
   - EntityName
   - EntityId
   - Action
   - ChangedBy
   - ChangedDate
   - OldValues
   - NewValues
===============================================================================
*/

SELECT TOP 100
    AuditLogID,
    EntityName,
    EntityId,
    Action,
    ChangedBy,
    ChangedDate,
    OldValues,
    NewValues
FROM dbo.AuditLogs
ORDER BY ChangedDate DESC;
GO