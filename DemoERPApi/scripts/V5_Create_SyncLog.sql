CREATE TABLE SyncLog
(
    LogId INT IDENTITY(1,1) PRIMARY KEY,

    CRMCustomerID NVARCHAR(50) NOT NULL,

    Operation NVARCHAR(20) NOT NULL,
    -- Insert
    -- Update
    -- Delete
    -- Sync


    Status NVARCHAR(20) NOT NULL,
    -- Success
    -- Failed
    -- Skipped


    Message NVARCHAR(500),

    Username NVARCHAR(100),

    RequestId NVARCHAR(100),

    CreatedDate DATETIME2 NOT NULL
        DEFAULT GETDATE(),

    ExecutionTimeMs INT
);
