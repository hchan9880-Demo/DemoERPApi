/*
===============================================================================
 DemoERPApi
 Script: SecurityValidationQueries.sql

 Description
 -----------
 Queries useful for validating authentication and refresh token data.
===============================================================================
*/

-- Active refresh tokens
SELECT *
FROM dbo.RefreshTokens
WHERE RevokedDate IS NULL
  AND ExpirationDate > SYSUTCDATETIME();
GO

-- Expired refresh tokens
SELECT *
FROM dbo.RefreshTokens
WHERE ExpirationDate <= SYSUTCDATETIME();
GO

-- Refresh tokens by user
SELECT
    UserId,
    COUNT(*) AS TokenCount
FROM dbo.RefreshTokens
GROUP BY UserId;
GO
