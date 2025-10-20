CREATE TABLE LawyerProfiles (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    Bio VARBINARY(2000),
    Spec VARCHAR(255),
    LicenseNum VARCHAR(255),
    ExpYears INT,
    Description VARBINARY(2000),
    Rating DECIMAL(19, 0),
    PricePerHour DECIMAL(19, 0),
    Img VARCHAR(255),
    DayOfWeek VARCHAR(255),
    WorkTime VARCHAR(255)
);

CREATE TABLE WorkSlots (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LawyerId INT,
    DayOfWeek VARCHAR(255),
    Slot VARCHAR(255),
    IsActive BIT,
    FOREIGN KEY (LawyerId) REFERENCES LawyerProfiles(Id)
);

-- Dữ liệu mẫu cho bảng LawyerProfiles
INSERT INTO LawyerProfiles (UserId, Bio, Spec, LicenseNum, ExpYears, Description, Rating, PricePerHour, Img, DayOfWeek, WorkTime)
VALUES
(1, 0x42696F31, 'Dân sự,Hợp đồng', 'LS1234', 10, 0x48C3A020CEBDB269204E1ED96969206D316E206BE1EC7269E1EC726E67207669C3AAC3AA6E67202E, 4.9, 500000, 'lawyer1.jpg', 'Mon,Tue,Wed', '08:00-12:00'),
(2, 0x42696F32, 'Hình sự,Tố tụng', 'LS5678', 8, 0x4DC3B42074E1BAA3204DC3B474206C75E1BAAD742073C6B02032, 4.7, 600000, 'lawyer2.jpg', 'Thu,Fri', '13:00-17:00');

-- Dữ liệu mẫu cho bảng WorkSlots
INSERT INTO WorkSlots (LawyerId, DayOfWeek, Slot, IsActive)
VALUES
(1, 'Thứ Hai', '09:00 - 10:00', 1),
(1, 'Thứ Ba', '14:00 - 15:00', 1),
(2, 'Thứ Tư', '10:00 - 11:00', 1),
(2, 'Thứ Năm', '16:00 - 17:00', 0);