-- =====================================
-- RESET CUSTOMER TEST DATA
-- =====================================

-- 1. Remove access mappings first (FK safety)
DELETE FROM CustomerAccess;

-- 2. Hard reset customers (cleanest for testing)
DELETE FROM Customer;

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