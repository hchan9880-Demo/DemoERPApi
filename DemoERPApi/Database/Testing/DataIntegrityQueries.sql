/*
===============================================================================
 DemoERPApi
 Script: DataIntegrityQueries.sql

 Description
 -----------
 Referential integrity and consistency checks.
===============================================================================
*/

-- Customers without user accounts
SELECT c.*
FROM dbo.Customers c
LEFT JOIN dbo.Users u
    ON c.CRMCustomerID = u.CustomerID
WHERE u.CustomerID IS NULL;
GO

-- Invalid customer access mappings
SELECT ca.*
FROM dbo.CustomerAccess ca
LEFT JOIN dbo.Customers c
    ON ca.CRMCustomerID = c.CRMCustomerID
WHERE c.CRMCustomerID IS NULL;
GO

-- Inactive users
SELECT *
FROM dbo.Users
WHERE IsActive = 0;
GO
