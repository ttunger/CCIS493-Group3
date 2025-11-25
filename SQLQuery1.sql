-- Select the correct database (ensure it exists in SQL Server)
USE HaircutBookingSystem;
GO

-- Drop-if-exists for reruns
IF OBJECT_ID('dbo.StylistNotes', 'U') IS NOT NULL DROP TABLE dbo.StylistNotes;
IF OBJECT_ID('dbo.Appointments', 'U') IS NOT NULL DROP TABLE dbo.Appointments;
IF OBJECT_ID('dbo.AppointmentTypes', 'U') IS NOT NULL DROP TABLE dbo.AppointmentTypes;
IF OBJECT_ID('dbo.Stylists', 'U') IS NOT NULL DROP TABLE dbo.Stylists;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- USERS table
CREATE TABLE dbo.Users
(
    UserID           INT IDENTITY(1,1) PRIMARY KEY,
    FullName         NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(255) NOT NULL,
    PasswordHash     NVARCHAR(255) NOT NULL,
    Role             NVARCHAR(20) NOT NULL
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Owner', N'Stylist', N'Client')),
    Phone            NVARCHAR(30),
    IsActive         BIT NOT NULL DEFAULT(1),
    CreatedAtUtc     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAtUtc     DATETIME2(0)
);
GO

CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email);
GO

-- STYLISTS
CREATE TABLE dbo.Stylists
(
    StylistID        INT IDENTITY(1,1) PRIMARY KEY,
    UserID           INT NOT NULL,
    Bio              NVARCHAR(500),
    Specialty        NVARCHAR(100),
    IsActive         BIT NOT NULL DEFAULT(1),
    CreatedAtUtc     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Stylists_User FOREIGN KEY (UserID)
        REFERENCES dbo.Users(UserID)
        ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX UX_Stylists_UserID ON dbo.Stylists(UserID);
GO

-- APPOINTMENT TYPES
CREATE TABLE dbo.AppointmentTypes
(
    TypeID           INT IDENTITY(1,1) PRIMARY KEY,
    Name             NVARCHAR(100) NOT NULL,
    Description      NVARCHAR(300),
    DurationMinutes  INT NOT NULL CHECK (DurationMinutes > 0),
    Price            DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    IsActive         BIT NOT NULL DEFAULT(1),
    CreatedAtUtc     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- APPOINTMENTS
CREATE TABLE dbo.Appointments
(
    AppointmentID    INT IDENTITY(1,1) PRIMARY KEY,
    ClientUserID     INT NOT NULL,
    StylistID        INT NOT NULL,
    TypeID           INT NOT NULL,
    StartDateTimeUtc DATETIME2(0) NOT NULL,
    EndDateTimeUtc   DATETIME2(0),
    Status           NVARCHAR(20) NOT NULL
        CHECK (Status IN (N'Pending', N'Confirmed', N'Completed', N'Cancelled', N'NoShow')),
    ClientNote       NVARCHAR(500),
    CreatedAtUtc     DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Appointments_ClientUser FOREIGN KEY (ClientUserID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Appointments_Stylist FOREIGN KEY (StylistID) REFERENCES dbo.Stylists(StylistID),
    CONSTRAINT FK_Appointments_Type FOREIGN KEY (TypeID) REFERENCES dbo.AppointmentTypes(TypeID)
);
GO

CREATE INDEX IX_Appointments_Stylist_Start ON dbo.Appointments(StylistID, StartDateTimeUtc);
CREATE INDEX IX_Appointments_Client_Start  ON dbo.Appointments(ClientUserID, StartDateTimeUtc);
GO

-- STYLIST NOTES
CREATE TABLE dbo.StylistNotes
(
    NoteID         INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentID  INT NOT NULL,
    StylistID      INT NOT NULL,
    NotesText      NVARCHAR(MAX) NOT NULL,
    CreatedAtUtc   DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_StylistNotes_Appt FOREIGN KEY (AppointmentID)
        REFERENCES dbo.Appointments(AppointmentID)
        ON DELETE CASCADE,
    CONSTRAINT FK_StylistNotes_Stylist FOREIGN KEY (StylistID)
        REFERENCES dbo.Stylists(StylistID)
);
GO

CREATE INDEX IX_StylistNotes_Appt ON dbo.StylistNotes(AppointmentID);
GO
