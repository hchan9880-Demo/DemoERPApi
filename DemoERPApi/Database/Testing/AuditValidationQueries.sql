/*
===============================================================================
DemoERPApi

Script:
    AuditValidationQueries.sql

Purpose:
    Validate AuditLogs records.

Coverage:

    1. Latest audit activity
    2. Customer history
    3. CREATE operations
    4. UPDATE operations
    5. DELETE operations
    6. User activity
    7. JSON value comparison

===============================================================================
*/


-- ============================================================
-- 1. Latest Audit Activity
-- ============================================================

SELECT *
FROM dbo.AuditLogs
ORDER BY ChangedDate DESC;



-- ============================================================
-- 2. Customer Audit History
-- ============================================================

SELECT *
FROM dbo.AuditLogs
WHERE EntityName = 'Customer'
AND EntityId = 'AUDIT_TEST_001'
ORDER BY ChangedDate;



-- ============================================================
-- 3. CREATE Operations
-- ============================================================

SELECT *
FROM dbo.AuditLogs
WHERE Action = 'CREATE'
ORDER BY ChangedDate DESC;



-- ============================================================
-- 4. UPDATE Operations
-- ============================================================

SELECT *
FROM dbo.AuditLogs
WHERE Action = 'UPDATE'
ORDER BY ChangedDate DESC;



-- ============================================================
-- 5. DELETE Operations
-- ============================================================

SELECT *
FROM dbo.AuditLogs
WHERE Action = 'DELETE'
ORDER BY ChangedDate DESC;



-- ============================================================
-- 6. User Activity
-- ============================================================

SELECT
    ChangedBy,
    COUNT(*) AS ChangeCount
FROM dbo.AuditLogs
GROUP BY ChangedBy
ORDER BY ChangeCount DESC;



-- ============================================================
-- 7. Compare Phone Changes
-- ============================================================

SELECT
    EntityId,

    JSON_VALUE(
        OldValues,
        '$.Phone'
    ) AS OldPhone,


    JSON_VALUE(
        NewValues,
        '$.Phone'
    ) AS NewPhone,


    ChangedBy,

    ChangedDate

FROM dbo.AuditLogs

WHERE Action='UPDATE';