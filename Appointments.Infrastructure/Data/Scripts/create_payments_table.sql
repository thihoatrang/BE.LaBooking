IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payments]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Payments](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] NVARCHAR(100) NOT NULL,
        [Vendor] NVARCHAR(20) NOT NULL,
        [Amount] BIGINT NULL,
        [Status] NVARCHAR(20) NOT NULL DEFAULT('pending'),
        [TransactionId] NVARCHAR(100) NULL,
        [Message] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt] DATETIME2 NULL
    );
    CREATE UNIQUE INDEX IX_Payments_OrderId ON [dbo].[Payments]([OrderId]);
END


