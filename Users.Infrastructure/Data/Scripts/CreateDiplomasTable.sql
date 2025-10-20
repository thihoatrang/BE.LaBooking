CREATE TABLE Diplomas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    LawyerId INT NOT NULL,
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
    CONSTRAINT FK_Diplomas_LawyerProfiles FOREIGN KEY (LawyerId) REFERENCES LawyerProfiles(Id)
);

-- Create index for faster lookups
CREATE INDEX IX_Diplomas_LawyerId ON Diplomas(LawyerId);
CREATE INDEX IX_Diplomas_IsDeleted ON Diplomas(IsDeleted);

GO

-- Add trigger to automatically update UpdatedAt
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