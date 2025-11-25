-- Seed data for HaircutBookingSystem (safe to run multiple times)
USE HaircutBookingSystem;
GO

-- Users (stylists)
IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Email='stylist1@local')
INSERT dbo.Users(FullName,Email,PasswordHash,Role,Phone)
VALUES ('Stylist One','stylist1@local','X','Stylist','');

IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE Email='stylist2@local')
INSERT dbo.Users(FullName,Email,PasswordHash,Role,Phone)
VALUES ('Stylist Two','stylist2@local','X','Stylist','');

-- Stylists mapping
IF NOT EXISTS(SELECT 1 FROM dbo.Stylists WHERE UserID = (SELECT UserID FROM dbo.Users WHERE Email='stylist1@local'))
INSERT dbo.Stylists(UserID,Bio,Specialty)
SELECT UserID,'','Cuts' FROM dbo.Users WHERE Email='stylist1@local';

IF NOT EXISTS(SELECT 1 FROM dbo.Stylists WHERE UserID = (SELECT UserID FROM dbo.Users WHERE Email='stylist2@local'))
INSERT dbo.Stylists(UserID,Bio,Specialty)
SELECT UserID,'','Color' FROM dbo.Users WHERE Email='stylist2@local';

-- Appointment types
IF NOT EXISTS(SELECT 1 FROM dbo.AppointmentTypes WHERE Name='Haircut')
INSERT dbo.AppointmentTypes(Name,Description,DurationMinutes,Price)
VALUES ('Haircut','Basic cut',30,25.00);

IF NOT EXISTS(SELECT 1 FROM dbo.AppointmentTypes WHERE Name='Hair Coloring')
INSERT dbo.AppointmentTypes(Name,Description,DurationMinutes,Price)
VALUES ('Hair Coloring','Single process',90,80.00);
GO

PRINT 'Seed complete.';
