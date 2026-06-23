SELECT * FROM Users;
SELECT * FROM Customer;
SELECT * FROM SyncLog;

SELECT *
FROM Customer
WHERE isDeleted=1;

SELECT *
FROM Customer
WHERE CONVERT(date, LastUpdated) = '2026-06-22';

SELECT *
FROM Customer
WHERE LastUpdated >= '2026-06-22'
  AND LastUpdated <  '2026-06-24';

SELECT *
FROM Customer
WHERE CONVERT(date, LastUpdate) = '2026-06-20';

SELECT *
FROM Customer
WHERE LastUpdate >= '2026-06-20'
  AND LastUpdate <  '2026-06-21';

/*
I prefer using a date range instead of CONVERT/CAST because it allows SQL Server to use indexes efficiently.
*/

/*

https://localhost:7087/swagger/index.html

TRUNCATE TABLE Customer;

INSERT INTO Customer (CRMCustomerID, FirstName, LastName, Email, Phone)
VALUES ('CRM100', 'John', 'Smith', 'john@email.com', '6041234567');

INSERT INTO Customer (CRMCustomerID, FirstName, LastName, Email, Phone)
VALUES ('CRM101', 'Alice', 'Brown', 'alice@email.com', '7785552222');

INSERT INTO Customer (CRMCustomerID, FirstName, LastName, Email, Phone)
VALUES ('CRM102', 'Michael', 'Johnson', 'michael.johnson@email.com', '6049998888');

INSERT INTO Customer (CRMCustomerID, FirstName, LastName, Email, Phone)
VALUES ('CRM103', 'David', 'Wilson', 'david.wilson@email.com', '7785553333');


SELECT * FROM Users;
3	admin1	HASHED_PASSWORD_HERE	Admin
4	qauser	Hxyz123123	QA


SELECT * FROM Customer;
1	CRM100	Michael	Johnson	michael@test.com	6049998888	NULL	0
2	string			test1	test2	2026-06-22 14:50:33.687	0
3	CRM102	Michael	Johnson	michael@test.com	6049998888	2026-06-22 19:40:48.417	1
4	string2	string	string	string@123.com	1234567890	2026-06-23 00:20:20.240	0
5	CRM200	Michael	Johnson	michael@test.com	6049998888	2026-06-23 00:36:16.420	0

Select * FROM SyncLog;
1	CRM100	Success	Inserted	2026-06-19 21:11:25.643
2	CRM101	Success	Inserted	2026-06-20 13:00:32.740
3	CRM101	Skipped	Already Exists	2026-06-20 13:01:06.590
4	CRM101	Skipped	Already Exists	2026-06-20 19:18:32.550
5	CRM100	Skipped	Already Exists	2026-06-20 19:21:50.547
6	CRM100	Skipped	Already Exists	2026-06-20 19:21:56.773
7	CRM102	Success	Inserted	2026-06-20 19:37:40.417
8	CRM102	Skipped	Already Exists	2026-06-20 19:37:48.193
9	CRM100	Skipped	Already Exists	2026-06-20 21:18:44.467
10	CRM103	Success	Inserted	2026-06-20 22:37:20.780
1009	string	Success	Inserted	2026-06-22 14:40:13.387
1010	string	Success	Inserted	2026-06-22 14:50:33.687
1011	CRM102	Success	Inserted	2026-06-22 19:40:48.417
1012	CRM102	Skipped	Already Exists	2026-06-22 19:54:25.090
1013	CRM102	Skipped	Already Exists	2026-06-22 19:55:31.030



*/