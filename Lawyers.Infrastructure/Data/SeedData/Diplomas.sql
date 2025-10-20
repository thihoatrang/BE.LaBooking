-- Insert sample data for Diplomas table
INSERT INTO Diplomas (LawyerId, Title, QualificationType, Description, IssuedDate, IssuedBy, DocumentUrl, IsPublic, IsVerified, CreatedAt, UpdatedAt, IsDeleted)
VALUES
-- Diplomas for Lawyer 1 (Nguyễn Văn A)
(1, 'Cử nhân Luật', 'Bachelor', 'Chuyên ngành Luật Dân sự', '2010-06-15', 'Đại học Luật Hà Nội', 'diplomas/lawyer1_bachelor.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(1, 'Thạc sĩ Luật', 'Master', 'Chuyên ngành Luật Dân sự', '2013-06-20', 'Đại học Luật Hà Nội', 'diplomas/lawyer1_master.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(1, 'Chứng chỉ Hành nghề Luật sư', 'License', 'Chứng chỉ hành nghề luật sư số LS1234', '2015-03-10', 'Liên đoàn Luật sư Việt Nam', 'diplomas/lawyer1_license.pdf', 1, 1, GETDATE(), GETDATE(), 0),

-- Diplomas for Lawyer 2 (Nguyễn Văn B)
(2, 'Cử nhân Luật', 'Bachelor', 'Chuyên ngành Luật Hình sự', '2012-06-15', 'Đại học Luật TP.HCM', 'diplomas/lawyer2_bachelor.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(2, 'Chứng chỉ Hành nghề Luật sư', 'License', 'Chứng chỉ hành nghề luật sư số LS5678', '2016-05-20', 'Liên đoàn Luật sư Việt Nam', 'diplomas/lawyer2_license.pdf', 1, 1, GETDATE(), GETDATE(), 0),

-- Diplomas for Lawyer 3 (Nguyễn Văn C)
(3, 'Cử nhân Luật', 'Bachelor', 'Chuyên ngành Luật Đất đai', '2011-06-15', 'Đại học Luật Đà Nẵng', 'diplomas/lawyer3_bachelor.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(3, 'Thạc sĩ Luật', 'Master', 'Chuyên ngành Luật Đất đai', '2014-06-20', 'Đại học Luật Đà Nẵng', 'diplomas/lawyer3_master.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(3, 'Chứng chỉ Hành nghề Luật sư', 'License', 'Chứng chỉ hành nghề luật sư số LS9001', '2016-08-15', 'Liên đoàn Luật sư Việt Nam', 'diplomas/lawyer3_license.pdf', 1, 1, GETDATE(), GETDATE(), 0),

-- Diplomas for Lawyer 4 (Nguyễn Văn D)
(4, 'Cử nhân Luật', 'Bachelor', 'Chuyên ngành Luật Doanh nghiệp', '2018-06-15', 'Đại học Luật Hà Nội', 'diplomas/lawyer4_bachelor.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(4, 'Chứng chỉ Hành nghề Luật sư', 'License', 'Chứng chỉ hành nghề luật sư số LS7004', '2020-03-10', 'Liên đoàn Luật sư Việt Nam', 'diplomas/lawyer4_license.pdf', 1, 1, GETDATE(), GETDATE(), 0),

-- Diplomas for Lawyer 5 (Trần Thị An)
(5, 'Cử nhân Luật', 'Bachelor', 'Chuyên ngành Luật Hôn nhân và Gia đình', '2016-06-15', 'Đại học Luật TP.HCM', 'diplomas/lawyer5_bachelor.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(5, 'Thạc sĩ Luật', 'Master', 'Chuyên ngành Luật Hôn nhân và Gia đình', '2019-06-20', 'Đại học Luật TP.HCM', 'diplomas/lawyer5_master.pdf', 1, 1, GETDATE(), GETDATE(), 0),
(5, 'Chứng chỉ Hành nghề Luật sư', 'License', 'Chứng chỉ hành nghề luật sư số LS3010', '2020-05-15', 'Liên đoàn Luật sư Việt Nam', 'diplomas/lawyer5_license.pdf', 1, 1, GETDATE(), GETDATE(), 0); 