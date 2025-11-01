-- Add AppointmentId column to Payments table
USE LA_Appointment;
GO

IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('dbo.Payments') 
               AND name = 'AppointmentId')
BEGIN
    ALTER TABLE [dbo].[Payments]
    ADD [AppointmentId] INT NULL;
    
    -- Add index for better query performance
    CREATE INDEX IX_Payments_AppointmentId ON [dbo].[Payments]([AppointmentId]);
    
    PRINT 'AppointmentId column added to Payments table';
END
ELSE
BEGIN
    PRINT 'AppointmentId column already exists in Payments table';
END
GO

