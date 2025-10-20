-- Create Lawyers table first
CREATE TABLE Lawyers (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    BarNumber NVARCHAR(50) NULL,
    Specialization NVARCHAR(255) NULL,
    YearsOfExperience INT NULL,
    Bio NVARCHAR(MAX) NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Lawyers_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create index for Lawyers table
CREATE INDEX IX_Lawyers_UserId ON Lawyers(UserId);
CREATE INDEX IX_Lawyers_IsDeleted ON Lawyers(IsDeleted);

GO

-- Create Diplomas table
CREATE TABLE Diplomas (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    LawyerId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    QualificationType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    IssuedDate DATETIME NULL,
    IssuedBy NVARCHAR(255) NULL,
    DocumentUrl NVARCHAR(MAX) NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    IsVerified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Diplomas_Lawyers FOREIGN KEY (LawyerId) REFERENCES Lawyers(Id)
);

-- Create index for Diplomas table
CREATE INDEX IX_Diplomas_LawyerId ON Diplomas(LawyerId);
CREATE INDEX IX_Diplomas_IsDeleted ON Diplomas(IsDeleted);

GO

-- Add trigger for Lawyers table
CREATE TRIGGER TR_Lawyers_UpdateTimestamp
ON Lawyers
AFTER UPDATE
AS
BEGIN
    UPDATE Lawyers
    SET UpdatedAt = GETDATE()
    FROM Lawyers l
    INNER JOIN inserted i ON l.Id = i.Id;
END;

GO

-- Add trigger for Diplomas table
CREATE TRIGGER TR_Diplomas_UpdateTimestamp
ON Diplomas
AFTER UPDATE
AS
BEGIN
    UPDATE Diplomas
    SET UpdatedAt = GETDATE()
    FROM Diplomas d
    INNER JOIN inserted i ON d.Id = i.Id;
END; 