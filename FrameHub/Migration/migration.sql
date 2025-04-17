-- MIGRATION SQL FILE .
-- MUST BE RUN FIRST BEFORE RUNNING THE APPLICATION

-- Database

    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FrameHub')
        BEGIN   
            CREATE DATABASE [FrameHub];
        END

-- Tables
        
    -- Users
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[Users] (
                Id            INT PRIMARY KEY IDENTITY(1,1),
                Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt     DATETIME2 NULL,
                Status        BIT NOT NULL DEFAULT 1,
                LastLogin     DATETIME2 NULL
            );
        END
    
    -- UserInfo
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserInfo' AND type = 'U')
        BEGIN
        CREATE TABLE [dbo].[UserInfo] (
                Id            INT PRIMARY KEY IDENTITY(1,1),
                Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt     DATETIME2 NULL,
                Status        BIT NOT NULL DEFAULT 1,
                UserId        INT NOT NULL UNIQUE,
                Username      NVARCHAR(50) NOT NULL UNIQUE,
                Email         NVARCHAR(100) NOT NULL UNIQUE,
                PhoneNumber   NVARCHAR(20) NULL,
                PasswordHash  NVARCHAR(256) NOT NULL
                );

            CONSTRAINT FK_UserInfo_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE  
        END
        
     -- UserSubscription    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserSubscription' AND type = 'U')
        BEGIN
        CREATE TABLE [dbo].[UserSubscription] (
            Id            INT PRIMARY KEY IDENTITY(1,1),
            Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt     DATETIME2 NULL,
            Status        BIT NOT NULL DEFAULT 1,
            UserId        INT NOT NULL UNIQUE,
            Plan          NVARCHAR(10) NOT NULL,
            AssignedAt    DATETIME2 NOT NULL,
            ExpiresAt     DATETIME2 NULL
            );

            CONSTRAINT FK_UserSubscription_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
        END
    
    -- Photos    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Photos' AND type = 'U')
        BEGIN
        CREATE TABLE [dbo].[Photos] (
            Id                  INT PRIMARY KEY IDENTITY(1,1),
            Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            CreatedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt           DATETIME2 NULL,
            Status              BIT NOT NULL DEFAULT 1,
            UserId              INT NOT NULL,
            StorageUrl          NVARCHAR(2048) NOT NULL,
            CdnUrl              NVARCHAR(2048) NOT NULL,
            Tags                NVARCHAR(500) NULL,
            IsProfilePicture    BIT NOT NULL,
            );

            CONSTRAINT FK_Photo_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
        END
    
    -- UserProfile    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserProfile' AND type = 'U')
        BEGIN
        CREATE TABLE [dbo].[UserProfile] (
            Id            INT PRIMARY KEY IDENTITY(1,1),
            Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
            CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt     DATETIME2 NULL,
            Status        BIT NOT NULL DEFAULT 1,
            UserId        INT NOT NULL UNIQUE,
            PictureId     INT NULL UNIQUE,
            DisplayName   NVARCHAR(50) NOT NULL,
            Description   NVARCHAR(500) NOT NULL
            );

            CONSTRAINT FK_UserProfile_Photos FOREIGN KEY (PictureId)
                REFERENCES [dbo].[Photos](Id) ON DELETE CASCADE
                                                       
            CONSTRAINT FK_UserProfile_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
        END