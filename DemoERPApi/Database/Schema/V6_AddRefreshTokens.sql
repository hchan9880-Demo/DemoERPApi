/*
===============================================================================
 DemoERPApi
 Database Version: v6.0
 Script: V6_AddRefreshTokens.sql

 Description
 -----------
 Adds the RefreshTokens table used for JWT refresh token authentication.
===============================================================================
*/

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NOT NULL
BEGIN
    PRINT 'RefreshTokens table already exists.';
    RETURN;
END
GO

CREATE TABLE dbo.RefreshTokens
(
    TokenId        INT IDENTITY(1,1) NOT NULL,
    UserId         INT NOT NULL,
    Token          NVARCHAR(500) NOT NULL,
    ExpirationDate DATETIME2 NOT NULL,
    CreatedDate    DATETIME2 NOT NULL
        CONSTRAINT DF_RefreshTokens_CreatedDate DEFAULT SYSUTCDATETIME(),
    RevokedDate    DATETIME2 NULL,
    ReplacedBy     NVARCHAR(500) NULL,

    CONSTRAINT PK_RefreshTokens PRIMARY KEY (TokenId),

    CONSTRAINT FK_RefreshTokens_Users
        FOREIGN KEY (UserId)
        REFERENCES dbo.Users(UserId)
);
GO

CREATE INDEX IX_RefreshTokens_UserId
ON dbo.RefreshTokens(UserId);
GO

PRINT 'RefreshTokens table created successfully.';
GO
