
-- Child tables first
DELETE FROM RefreshTokens;

DELETE FROM AuditLogs;
DELETE FROM CustomerAccess;
DELETE FROM SyncLogs;

-- Parent tables
DELETE FROM Users;
DELETE FROM Customers;


