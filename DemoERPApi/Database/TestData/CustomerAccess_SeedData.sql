/*
===============================================================================
 DemoERPApi
 Script: CustomerAccess_SeedData.sql

 Description
 -----------
 Grants users access to customer records.
===============================================================================
*/

DELETE FROM dbo.CustomerAccess;
GO

INSERT INTO dbo.CustomerAccess
(
    UserId,
    CRMCustomerID,
    GrantedBy
)
VALUES
(2, 'CRM103', 'System'),
(3, 'CRM104', 'System'),
(4, 'CRM105', 'System'),
(5, 'CRM106', 'System'),
(6, 'CRM300', 'System'),
(7, 'CRM301', 'System');
GO
