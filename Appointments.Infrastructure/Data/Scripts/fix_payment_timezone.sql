-- Fix Payment CreatedAt timezone from UTC to local time (Vietnam UTC+7)
USE LA_Appointment;
GO

-- Drop existing default constraint
IF EXISTS (
    SELECT * FROM sys.default_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Payments') 
    AND name = 'DF__Payments__Create__[constraint_name]'
)
BEGIN
    DECLARE @constraint_name NVARCHAR(200);
    SELECT @constraint_name = name 
    FROM sys.default_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Payments') 
    AND definition LIKE '%sysutcdatetime%';
    
    IF @constraint_name IS NOT NULL
    BEGIN
        DECLARE @sql NVARCHAR(MAX) = 'ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [' + @constraint_name + ']';
        EXEC sp_executesql @sql;
        PRINT 'Dropped existing default constraint for CreatedAt';
    END
END

-- Alternative: Drop by checking the definition
DECLARE @constraintName NVARCHAR(200);
SELECT @constraintName = dc.name
FROM sys.default_constraints dc
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Payments')
  AND c.name = 'CreatedAt'
  AND dc.definition LIKE '%sysutcdatetime%';

IF @constraintName IS NOT NULL
BEGIN
    DECLARE @dropSql NVARCHAR(MAX) = 'ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [' + @constraintName + ']';
    EXEC sp_executesql @dropSql;
    PRINT 'Dropped constraint: ' + @constraintName;
END

-- Add new default constraint with GETDATE() (local time)
IF NOT EXISTS (
    SELECT * FROM sys.default_constraints 
    WHERE parent_object_id = OBJECT_ID('dbo.Payments') 
    AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.Payments'), 'CreatedAt', 'ColumnId')
    AND definition LIKE '%GETDATE%'
)
BEGIN
    ALTER TABLE [dbo].[Payments]
    ADD CONSTRAINT DF_Payments_CreatedAt DEFAULT (GETDATE()) FOR [CreatedAt];
    PRINT 'Added new default constraint with GETDATE() for CreatedAt';
END
ELSE
BEGIN
    PRINT 'Default constraint with GETDATE() already exists';
END
GO

-- Note: Existing records will keep their UTC time
-- Only new records will use local time (GETDATE())

