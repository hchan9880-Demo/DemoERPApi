-- Customers without user accounts

SELECT *
FROM Customers c
LEFT JOIN Users u
ON c.CRMCustomerID=u.CustomerID
WHERE u.CustomerID IS NULL;

-- Invalid customer access mappings

SELECT *
FROM CustomerAccess ca
LEFT JOIN Customers c
ON ca.CRMCustomerID=c.CRMCustomerID
WHERE c.CRMCustomerID IS NULL;

-- Inactive users

SELECT *
FROM Users
WHERE IsActive=0;