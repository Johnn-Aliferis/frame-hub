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
                Id            BIGINT PRIMARY KEY IDENTITY(1,1),
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
                Id            BIGINT PRIMARY KEY IDENTITY(1,1),
                Guid          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt     DATETIME2 NULL,
                Status        BIT NOT NULL DEFAULT 1,
                UserId        BIGINT NOT NULL,
                Email         NVARCHAR(100) NOT NULL,
                PasswordHash  NVARCHAR(256) NOT NULL,
                Provider      NVARCHAR(50) NOT NULL,
                ExternalId    NVARCHAR(100) NOT NULL

            CONSTRAINT FK_UserCredential_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
    
            );

            CREATE UNIQUE INDEX IX_UserCredential_Email
                ON [dbo].[UserCredential] ([Email]);
            
            CREATE UNIQUE INDEX IX_UserCredential_UserId
                ON [dbo].[UserCredential] ([UserId]);
        END
            
    -- Photo    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Photo' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[Photo] (
                Id                  BIGINT PRIMARY KEY IDENTITY(1,1),
                Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt           DATETIME2 NULL,
                Status              BIT NOT NULL DEFAULT 1,
                UserId              BIGINT NOT NULL,
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
                    Id               BIGINT PRIMARY KEY IDENTITY(1,1),
                    Guid             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                    CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedAt        DATETIME2 NULL,
                    Status           BIT NOT NULL DEFAULT 1,
                    DisplayName      NVARCHAR(50) NOT NULL,
                    PhoneNumber      NVARCHAR(20) NULL,
                    Bio              NVARCHAR(500) NULL,
                    ProfilePictureId BIGINT NULL,
                    UserId           BIGINT NOT NULL
    
            CONSTRAINT FK_UserInfo_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE
                
            );

            CREATE UNIQUE INDEX IX_UserInfo_UserId
                ON [dbo].[UserInfo] ([UserId]);
        END

    -- SubscriptionPlan
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'SubscriptionPlan' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[SubscriptionPlan] (
                Id               BIGINT PRIMARY KEY IDENTITY(1,1),
                Guid             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt        DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt        DATETIME2 NULL,
                Status           BIT NOT NULL DEFAULT 1,
                Code             NVARCHAR(20) NOT NULL,
                Name             NVARCHAR(50) NOT NULL,
                Description      NVARCHAR(200) NULL,
                MaxUploads       BIGINT NOT NULL,
                MonthlyPrice     DECIMAL(10,2) NULL
            );

            CREATE UNIQUE INDEX IX_SubscriptionPlan_Code
                ON [dbo].[SubscriptionPlan] ([Code]);

            -- Seed initial data
            INSERT INTO [dbo].[SubscriptionPlan] (Code, Name, Description, MaxUploads, MonthlyPrice)
            VALUES
                ('Basic', 'Basic Plan', 'Free plan with limited access. Preview only.', 0, 0.00),
                ('Pro', 'Pro Plan', 'Paid plan with extended uploads and features.', 3, 9.99),
                ('Ultimate', 'Ultimate Plan', 'Maximum access to all features.', 20, 29.99);
        END
        
     -- UserSubscription    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserSubscription' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[UserSubscription] (
                Id                  BIGINT PRIMARY KEY IDENTITY(1,1),
                Guid                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt           DATETIME2 NULL,
                Status              BIT NOT NULL DEFAULT 1,
                UserId              BIGINT NOT NULL,
                SubscriptionPlanId  BIGINT NOT NULL,
                AssignedAt          DATETIME2 NOT NULL,
                ExpiresAt           DATETIME2 NOT NULL
    
            CONSTRAINT FK_UserSubscription_Users FOREIGN KEY (UserId)
                REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
                
            CONSTRAINT FK_UserSubscription_Plans FOREIGN KEY (SubscriptionPlanId)
                REFERENCES SubscriptionPlan(Id) ON DELETE CASCADE

            );

            CREATE UNIQUE INDEX IX_UserSubscription_UserId
                ON [dbo].[UserSubscription] ([UserId]);
        END