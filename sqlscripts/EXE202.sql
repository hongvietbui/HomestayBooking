CREATE DATABASE TamDaoStay_DB;
GO
USE TamDaoStay_DB;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Customer')
BEGIN
    CREATE TABLE Customer
    (
        CustomerID       INT IDENTITY PRIMARY KEY,
        FirstName        NVARCHAR(50) NOT NULL,
        LastName         NVARCHAR(50) NOT NULL,
        DateOfBirth      DATE NOT NULL,
        PhoneNumber      NVARCHAR(20) NOT NULL,
        Email            NVARCHAR(100) NOT NULL UNIQUE,
        Address          NVARCHAR(255) NOT NULL,
        Password         NVARCHAR(255) NOT NULL,
        Balance          DECIMAL(18, 2) DEFAULT 0 NOT NULL,
        PasswordAttempts INT DEFAULT 0 NOT NULL,
        UnlockTime       DATETIME,
        Role             INT NOT NULL,
        Image            NVARCHAR(500),
        IsActive         BIT DEFAULT 1
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BookingContract')
BEGIN
    CREATE TABLE BookingContract
    (
        BookingID     INT IDENTITY PRIMARY KEY,
        RoomID        INT NOT NULL,
        CustomerID    INT NOT NULL
            REFERENCES Customer(CustomerID)
                ON DELETE CASCADE,
        FirstName     NVARCHAR(50) NOT NULL,
        LastName      NVARCHAR(50) NOT NULL,
        Email         NVARCHAR(50) NOT NULL,
        Phone         NVARCHAR(50) NOT NULL,
        Destination   NVARCHAR(50) NOT NULL,
        StartDate     DATE NOT NULL,
        EndDate       DATE NOT NULL,
        TotalAmount   DECIMAL(18, 2) NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        Note          NVARCHAR(MAX),
        Status        NVARCHAR(50) NOT NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Host')
BEGIN
    CREATE TABLE Host
    (
        HostID      INT IDENTITY PRIMARY KEY,
        FirstName   NVARCHAR(50) NOT NULL,
        LastName    NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Email       NVARCHAR(100) NOT NULL UNIQUE,
        Address     NVARCHAR(255) NOT NULL,
        Password    NVARCHAR(255) NOT NULL,
        CreatedAt   DATETIME DEFAULT GETDATE()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Homestay')
BEGIN
    CREATE TABLE Homestay
    (
        HomestayID         INT IDENTITY PRIMARY KEY,
        HostID             INT NOT NULL
            REFERENCES Host(HostID)
                ON DELETE CASCADE,
        Name               NVARCHAR(255) NOT NULL,
        Description        NVARCHAR(MAX),
        Address            NVARCHAR(MAX) NOT NULL,
        City               NVARCHAR(255) NOT NULL,
        Country            VARCHAR(255) NOT NULL,
        PricePerNight      DECIMAL(10, 2) NOT NULL,
        MaxGuests          INT NOT NULL,
        Wifi               BIT DEFAULT 0,
        Cafe               BIT DEFAULT 0,
        AirConditional     BIT DEFAULT 0,
        Parking            BIT DEFAULT 0,
        Pool               BIT DEFAULT 0,
        Kitchen            BIT DEFAULT 0,
        PetFriendly        BIT DEFAULT 0,
        Laundry            BIT DEFAULT 0,
        BreakfirstIncluded BIT DEFAULT 0,
        Gym                BIT DEFAULT 0,
        SmokingAllowed     BIT DEFAULT 0,
        Balcony            BIT DEFAULT 0,
        Status             BIT DEFAULT 1,
        CreatedAt          DATETIME DEFAULT GETDATE(),
        RoomSize           NVARCHAR(MAX)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Feedback')
BEGIN
    CREATE TABLE Feedback
    (
        FeedbackID      INT IDENTITY PRIMARY KEY,
        CustomerID      INT NOT NULL
            REFERENCES Customer(CustomerID)
                ON DELETE CASCADE,
        HomestayID      INT NOT NULL
            REFERENCES Homestay(HomestayID)
                ON DELETE CASCADE,
        FeedbackContent NVARCHAR(1000),
        Rating          INT CHECK (Rating >= 1 AND Rating <= 5),
        FeedbackDate    DATE DEFAULT GETDATE() NOT NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Image_Homestay')
BEGIN
    CREATE TABLE Image_Homestay
    (
        ImageID    INT IDENTITY PRIMARY KEY,
        HomestayID INT NOT NULL
            REFERENCES Homestay(HomestayID)
                ON DELETE CASCADE,
        Image1     TEXT NOT NULL,
        Image2     TEXT,
        Image3     TEXT,
        Image4     TEXT,
        Image5     TEXT,
        CreatedAt  DATETIME DEFAULT GETDATE()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Transaction')
BEGIN
    CREATE TABLE Transactions
    (
        ID         INT IDENTITY PRIMARY KEY,
        CustomerID INT NOT NULL
            REFERENCES Customer(CustomerID)
                ON DELETE CASCADE,
        IssueDate  DATETIME DEFAULT GETDATE() NOT NULL,
        Action     DECIMAL(18, 2) NOT NULL,
        Note       NVARCHAR(255)
    );
END;
GO
