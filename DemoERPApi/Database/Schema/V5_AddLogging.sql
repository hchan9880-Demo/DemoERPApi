/*
===============================================================================
 DemoERPApi
 Script: V5_AddLogging.sql

 Description
 -----------
 Adds application-level logging support for customer synchronization operations.

 Purpose
 -------
 The SyncLogs table provides operational visibility into API execution,
 synchronization processes, and business workflow outcomes.

 This table supports:
   - API request tracking
   - Customer synchronization monitoring
   - Error investigation
   - Performance analysis
   - Production troubleshooting
   - Audit and compliance reporting

 Difference from AuditLogs:
   AuditLogs  -> Tracks DATA changes (before/after values)
   SyncLogs   -> Tracks APPLICATION events and execution results

===============================================================================
*/





/*
===============================================================================
 Create Indexes

 Improve query performance for common production support scenarios.
===============================================================================
*/


-- Find all logs for a specific customer
CREATE INDEX IX_SyncLogs_CRMCustomerID
ON dbo.SyncLogs(CRMCustomerID);
GO



-- Search logs by operation result
CREATE INDEX IX_SyncLogs_Status
ON dbo.SyncLogs(Status);
GO



-- Query recent application activity
CREATE INDEX IX_SyncLogs_CreatedDate
ON dbo.SyncLogs(CreatedDate DESC);
GO



-- Trace requests across API calls
CREATE INDEX IX_SyncLogs_RequestId
ON dbo.SyncLogs(RequestId);
GO



/*
===============================================================================
 Insert Sample Validation Records

 Purpose:
   Verify table creation and demonstrate expected logging format.

 These records simulate:
   - Successful customer creation
   - Duplicate customer warning
   - Validation failure
===============================================================================
*/


INSERT INTO dbo.SyncLogs
(
    CRMCustomerID,
    Operation,
    Status,
    Message,
    Username,
    RequestId,
    ExecutionTimeMs
)
VALUES
(
    'CRM100',
    'CREATE',
    'SUCCESS',
    'Customer created successfully',
    'admin',
    'REQ-001',
    25
),
(
    'CRM101',
    'CREATE',
    'WARNING',
    'Duplicate customer submitted',
    'admin',
    'REQ-002',
    15
),
(
    'UNKNOWN',
    'CREATE',
    'FAILED',
    'CustomerID required',
    'Unknown',
    'REQ-003',
    5
);
GO



/*
===============================================================================
 Validation Queries

 Confirm logging functionality.
===============================================================================
*/


-- View latest application activity

SELECT *
FROM dbo.SyncLogs
ORDER BY CreatedDate DESC;
GO



-- View failed operations

SELECT *
FROM dbo.SyncLogs
WHERE Status = 'FAILED'
ORDER BY CreatedDate DESC;
GO



-- View warning conditions

SELECT *
FROM dbo.SyncLogs
WHERE Status = 'WARNING'
ORDER BY CreatedDate DESC;
GO



-- Performance monitoring

SELECT
    Operation,
    AVG(ExecutionTimeMs) AS AverageExecutionTimeMs,
    MAX(ExecutionTimeMs) AS MaximumExecutionTimeMs,
    COUNT(*) AS TotalRequests
FROM dbo.SyncLogs
GROUP BY Operation;
GO