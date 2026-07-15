/*
===============================================================================
 DemoERPApi
 Database Version: v6.0
 Script: V6_AddAuditLogging.sql

 Description
 -----------
 Adds the AuditLogs table used to record customer Create, Update,
 and Delete operations for auditing and troubleshooting.
===============================================================================
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL
BEGIN
    PRINT 'AuditLogs table already exists.';
    RETURN;
END
GO

CREATE TABLE dbo.AuditLogs
(
    AuditId      INT IDENTITY(1,1) NOT NULL,
    EntityName   NVARCHAR(100) NOT NULL,
    EntityId     NVARCHAR(100) NOT NULL,
    Action       NVARCHAR(20) NOT NULL,
    OldValues    NVARCHAR(MAX) NULL,
    NewValues    NVARCHAR(MAX) NULL,
    ChangedBy    NVARCHAR(200) NULL,
    RequestId    NVARCHAR(100) NULL,
    IPAddress    NVARCHAR(50) NULL,
    ChangedDate  DATETIME2 NOT NULL
        CONSTRAINT DF_AuditLogs_ChangedDate DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_AuditLogs PRIMARY KEY (AuditId)
);
GO

CREATE INDEX IX_AuditLogs_Entity
ON dbo.AuditLogs(EntityName, EntityId);
GO

CREATE INDEX IX_AuditLogs_ChangedDate
ON dbo.AuditLogs(ChangedDate DESC);
GO

PRINT 'AuditLogs table created successfully.';
GO
