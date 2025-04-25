-- MIGRATION SQL FILE .
-- MUST BE RUN FIRST BEFORE RUNNING THE APPLICATION

-- Database

    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FrameHub')
        BEGIN   
            CREATE DATABASE [FrameHub];
        END
    GO 
   
    -- Switch to the FrameHub database
    USE FrameHub;
    GO 
    
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
    
     -- UserCredential    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserCredential' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[UserCredential] (
                Id            INT PRIMARY KEY IDENTITY(1,1),
                Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt     DATETIME2 NULL,
                Status        BIT NOT NULL DEFAULT 1,
                UserId        INT NOT NULL UNIQUE,
                Email         NVARCHAR(100) NOT NULL UNIQUE,
                PasswordHash  NVARCHAR(256) NOT NULL

                CONSTRAINT FK_UserCredential_Users FOREIGN KEY (UserId)
                    REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
            );
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
                IsProfilePicture    BIT NOT NULL

            CONSTRAINT FK_Photo_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE  
            );
        END
    
    -- UserInfo
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserInfo' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[UserInfo] (
                    Id               INT PRIMARY KEY IDENTITY(1,1),
                    Guid             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedAt        DATETIME2 NULL,
                    Status           BIT NOT NULL DEFAULT 1,
                    DisplayName      NVARCHAR(50) NOT NULL UNIQUE,
                    PhoneNumber      NVARCHAR(20) NULL,
                    Bio              NVARCHAR(500) NULL,
                    ProfilePictureId INT NOT NULL UNIQUE,
                    UserId           INT NOT NULL UNIQUE
    
            CONSTRAINT FK_UserInfo_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
            );
        END

    -- SubscriptionPlans
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SubscriptionPlans' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[SubscriptionPlans] (
                Id               INT PRIMARY KEY IDENTITY(1,1),
                Guid             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt        DATETIME2 NULL,
                Status           BIT NOT NULL DEFAULT 1,
                Code             NVARCHAR(20) NOT NULL UNIQUE,
                Name             NVARCHAR(50) NOT NULL,
                Description      NVARCHAR(200) NULL,
                MaxUploads       INT NOT NULL,
                MonthlyPrice     DECIMAL(10,2) NULL
            );

            -- Seed initial data
            INSERT INTO [dbo].[SubscriptionPlans] (Code, Name, Description, MaxUploads, MonthlyPrice)
            VALUES
                ('Basic', 'Basic Plan', 'Free plan with limited access. Preview only.', 0, 0.00),
                ('Pro', 'Pro Plan', 'Paid plan with extended uploads and features.', 3, 9.99),
                ('Ultimate', 'Ultimate Plan', 'Maximum access to all features.', 20, 29.99);
        END
        
     -- UserSubscription    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserSubscription' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[UserSubscription] (
                Id                  INT PRIMARY KEY IDENTITY(1,1),
                Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt           DATETIME2 NULL,
                Status              BIT NOT NULL DEFAULT 1,
                UserId              INT NOT NULL UNIQUE,
                SubscriptionPlanId  INT NOT NULL,
                AssignedAt          DATETIME2 NOT NULL,
                ExpiresAt           DATETIME2 NULL
    
            CONSTRAINT FK_UserSubscription_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
                
            CONSTRAINT FK_UserSubscription_Plans FOREIGN KEY (SubscriptionPlanId)
                REFERENCES SubscriptionPlans(Id) ON DELETE CASCADE
            );
        END