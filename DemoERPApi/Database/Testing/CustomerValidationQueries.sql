-- Verify customer count

SELECT COUNT(*)
FROM Customers;

-- Verify duplicate customer IDs

SELECT
CRMCustomerID,
COUNT(*)
FROM Customers
GROUP BY CRMCustomerID
HAVING COUNT(*) > 1;

-- Verify duplicate customer IDs

SELECT
CRMCustomerID,
COUNT(*)
FROM Customers
GROUP BY CRMCustomerID
HAVING COUNT(*) > 1;

-- Verify active customers

SELECT *
FROM Customers
WHERE IsDeleted = 0;

-- Verify soft deleted customers

SELECT *
FROM Customers
WHERE IsDeleted = 1;

-- Verify invalid emails

SELECT *
FROM Customers
WHERE Email NOT LIKE '%@%';


