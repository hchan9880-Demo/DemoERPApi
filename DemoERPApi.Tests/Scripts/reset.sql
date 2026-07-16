-- =====================================
-- RESET CUSTOMER TEST DATA
-- =====================================

-- Child tables first
DELETE FROM RefreshTokens;

DELETE FROM AuditLogs;
DELETE FROM CustomerAccess;
DELETE FROM SyncLogs;

-- Parent tables
DELETE FROM Users;
DELETE FROM Customers;










-- 3. Re-seed known baseline customers
INSERT INTO Customer
(CRMCustomerID, FirstName, LastName, Email, Phone, CreatedDate, IsDeleted)
VALUES
('CRM100', 'Michael', 'Johnson', 'michael@test.com', '6049998888', GETDATE(), 0);

INSERT INTO Customer
(CRMCustomerID, FirstName, LastName, Email, Phone, CreatedDate, IsDeleted)
VALUES
('CRM103', 'Ada', 'Smith', 'asmith1@test.com', '6049998876', GETDATE(), 0);

INSERT INTO Customer
(CRMCustomerID, FirstName, LastName, Email, Phone, CreatedDate, IsDeleted)
VALUES
('CRM200', 'Michael', 'Johnson', 'michael@test.com', '6049998888', GETDATE(), 0);



