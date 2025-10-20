CREATE DATABASE LA_Saga;
GO

USE LA_Saga;
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SagaStates]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[SagaStates] (
		[Id] NVARCHAR(50) NOT NULL PRIMARY KEY,
		[SagaType] NVARCHAR(50) NOT NULL,
		[EntityId] NVARCHAR(50) NOT NULL,
		[State] NVARCHAR(50) NOT NULL,
		[Data] NVARCHAR(MAX) NULL,
		[ErrorMessage] NVARCHAR(1000) NULL,
		[CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
		[CompletedAt] DATETIME2 NULL,
		[FailedAt] DATETIME2 NULL,
		[LastUpdatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
	);

	CREATE INDEX [IX_SagaStates_SagaType_EntityId] ON [dbo].[SagaStates] ([SagaType], [EntityId]);
	CREATE INDEX [IX_SagaStates_CreatedAt] ON [dbo].[SagaStates] ([CreatedAt]);
END
GO
