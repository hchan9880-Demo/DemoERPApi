USE DemoERP;
GO

-- Delete all data
DELETE FROM Customers;
GO

-- Reset identity seed
DBCC CHECKIDENT ('Customers', RESEED, 0);
GO