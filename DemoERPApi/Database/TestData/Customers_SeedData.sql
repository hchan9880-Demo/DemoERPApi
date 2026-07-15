/*
===============================================================================
 DemoERPApi
 Script: Customers_SeedData.sql

 Description
 -----------
 Inserts sample customer records used for development and testing.
===============================================================================
*/

DELETE FROM dbo.Customers;
GO

INSERT INTO dbo.Customers
(
    CRMCustomerID,
    FirstName,
    LastName,
    Email,
    Phone,
    IsDeleted,
    LastUpdated
)
VALUES
('CRM001','Admin','User','admin@test.com','6040000001',0,SYSUTCDATETIME()),
('CRM100','Michael','Test','michael@test.com','6041111100',0,SYSUTCDATETIME()),
('CRM101','Sarah','Test','sarah@test.com','6042222101',0,SYSUTCDATETIME()),
('CRM103','Owner','User','owner@test.com','6043333103',0,SYSUTCDATETIME()),
('CRM104','Customer','One','customer@test.com','6043333104',0,SYSUTCDATETIME()),
('CRM105','QA','UserA','qauserA@test.com','6044444105',0,SYSUTCDATETIME()),
('CRM106','QA','UserB','qauserB@test.com','6044444106',0,SYSUTCDATETIME()),
('CRM300','Other','User','other@test.com','6044444300',0,SYSUTCDATETIME()),
('CRM301','Owner2','User','owner2@test.com','6045555301',0,SYSUTCDATETIME());
GO
