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
    
    -- AspNetRoles
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetRoles' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetRoles] (
                [Id] nvarchar(450) NOT NULL,
                [Name] nvarchar(256) NULL,
                [NormalizedName] nvarchar(256) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
            );
        END
    

    -- AspNetUsers
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUsers' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetUsers] (
                [Id] nvarchar(450) NOT NULL,
                [UserName] nvarchar(256) NULL,
                [NormalizedUserName] nvarchar(256) NULL,
                [Email] nvarchar(256) NULL,
                [NormalizedEmail] nvarchar(256) NULL,
                [EmailConfirmed] bit NOT NULL,
                [PasswordHash] nvarchar(max) NULL,
                [SecurityStamp] nvarchar(max) NULL,
                [ConcurrencyStamp] nvarchar(max) NULL,
                [PhoneNumber] nvarchar(max) NULL,
                [PhoneNumberConfirmed] bit NOT NULL,
                [TwoFactorEnabled] bit NOT NULL,
                [LockoutEnd] datetimeoffset NULL,
                [LockoutEnabled] bit NOT NULL,
                [AccessFailedCount] int NOT NULL,
                [Discriminator] NVARCHAR(256) NOT NULL DEFAULT 'ApplicationUser',
                [Guid]                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                [CreatedAt]           DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                [UpdatedAt]           DATETIME2 NULL,
                [Status]              BIT NOT NULL DEFAULT 1,
                CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
            );
        END
        
    -- AspNetRoleClaims
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetRoleClaims' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetRoleClaims] (
                [Id] int NOT NULL IDENTITY,
                [RoleId] nvarchar(450) NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
            );
        END
    
    -- AspNetUserClaims
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserClaims' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetUserClaims] (
                [Id] int NOT NULL IDENTITY,
                [UserId] nvarchar(450) NOT NULL,
                [ClaimType] nvarchar(max) NULL,
                [ClaimValue] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
                CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
        END
    
    -- AspNetUserLogins
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserLogins' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetUserLogins] (
                [LoginProvider] nvarchar(450) NOT NULL,
                [ProviderKey] nvarchar(450) NOT NULL,
                [ProviderDisplayName] nvarchar(max) NULL,
                [UserId] nvarchar(450) NOT NULL,
                CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY NONCLUSTERED ([LoginProvider], [ProviderKey]),
                CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
        END
    
    -- AspNetUserRoles
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserRoles' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetUserRoles] (
                [UserId] nvarchar(450) NOT NULL,
                [RoleId] nvarchar(450) NOT NULL,
                CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
                CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
        END
        
    -- AspNetUserTokens
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AspNetUserTokens' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[AspNetUserTokens] (
                [UserId] nvarchar(450) NOT NULL,
                [LoginProvider] nvarchar(450) NOT NULL,
                [Name] nvarchar(450) NOT NULL,
                [Value] nvarchar(max) NULL,
                CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY NONCLUSTERED ([UserId], [LoginProvider], [Name]),
                CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
            );
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
                UserId              NVARCHAR(450) NOT NULL,
                StorageUrl          NVARCHAR(2048) NOT NULL,
                CdnUrl              NVARCHAR(2048) NOT NULL,
                Tags                NVARCHAR(500) NULL,
                IsProfilePicture    BIT NOT NULL

            CONSTRAINT FK_Photo_AspNetUsers FOREIGN KEY (UserId)
                REFERENCES [dbo].[AspNetUsers](Id) ON DELETE CASCADE
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
                    UserId           NVARCHAR(450) NOT NULL
    
            CONSTRAINT FK_UserInfo_AspNetUsers FOREIGN KEY (UserId)
                REFERENCES [dbo].[AspNetUsers](Id) ON DELETE CASCADE
                
            );
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
                ProductId  NVARCHAR(100) NULL,
                PriceId    NVARCHAR(100) NULL,
                Description      NVARCHAR(200) NULL,
                MaxUploads       INT NOT NULL,
                MonthlyPrice     DECIMAL(10,2) NULL
            );
        END
        
     -- UserSubscription    
    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserSubscription' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[UserSubscription] (
                Id                   BIGINT PRIMARY KEY IDENTITY(1,1),
                Guid                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                CreatedAt            DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                UpdatedAt            DATETIME2 NULL,
                Status               BIT NOT NULL DEFAULT 1,
                UserId               NVARCHAR(450) NOT NULL,
                SubscriptionPlanId   BIGINT NOT NULL,
                CustomerId           NVARCHAR(100) NULL,
                SubscriptionId       NVARCHAR(100) NULL,
                PaymentStatus        NVARCHAR(50) NULL,
                AssignedAt           DATETIME2 NOT NULL,
                ExpiresAt            DATETIME2 NULL
    
            CONSTRAINT FK_UserSubscription_AspNetUsers FOREIGN KEY (UserId)
                REFERENCES [dbo].[AspNetUsers](Id) ON DELETE CASCADE,
                
            CONSTRAINT FK_UserSubscription_Plans FOREIGN KEY (SubscriptionPlanId)
                REFERENCES SubscriptionPlan(Id) ON DELETE CASCADE

            );
        END
        
          -- UserTransactionHistory   
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'UserTransactionHistory' AND type = 'U')
            BEGIN
                CREATE TABLE [dbo].[UserTransactionHistory] (
                    Id                     BIGINT PRIMARY KEY IDENTITY(1,1),
                    Guid                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
                    CreatedAt              DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedAt              DATETIME2 NULL,
                    Status                 BIT NOT NULL DEFAULT 1,
                    UserId                 NVARCHAR(450) NOT NULL,
                    Amount                 DECIMAL(10,2) NOT NULL,
                    Currency               NVARCHAR(10) NOT NULL,
                    PaymentIntentId        NVARCHAR(255) NOT NULL,
                    InvoiceId              NVARCHAR(255) NULL,
                    Description            NVARCHAR(255) NULL,
                    ReceiptUrl             NVARCHAR(2048) NULL,
                    MetadataJson           NVARCHAR(MAX) NULL,
                
                    CONSTRAINT FK_UserTransactionHistory_AspNetUsers FOREIGN KEY (UserId)
                    REFERENCES [dbo].[AspNetUsers](Id) ON DELETE CASCADE
                );
            END
        
        -- WebhookEvent  
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'WebhookEvent' AND type = 'U')
        BEGIN
            CREATE TABLE [dbo].[WebhookEvent] (
                Id              BIGINT PRIMARY KEY IDENTITY(1,1),
                EventId   NVARCHAR(100) NOT NULL,
                EventType       NVARCHAR(100) NOT NULL,
                ReceivedAt      DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                RawPayload      NVARCHAR(MAX) NOT NULL,
                Processed       BIT NOT NULL DEFAULT 0
            );
        END

-- ========================
-- Indexes
-- ========================
-- Identity Indexes
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

-- Custom Table Indexes
CREATE UNIQUE INDEX IX_UserInfo_UserId ON [UserInfo] (UserId);
CREATE UNIQUE INDEX IX_UserSubscription_UserId ON [UserSubscription] (UserId);
CREATE UNIQUE INDEX IX_SubscriptionPlan_Code ON [SubscriptionPlan] (Code);
CREATE UNIQUE INDEX IX_WebhookEvent_EventId ON [WebhookEvent] (EventId);
CREATE INDEX  IX_UserTransactionHistory_UserId ON [UserTransactionHistory] (UserId);

-- ========================
-- Seed Data
-- ========================
INSERT INTO [dbo].[SubscriptionPlan] (Code, Name, ProductId, PriceId,  Description, MaxUploads, MonthlyPrice)
VALUES
    ('BASIC', 'Basic Plan', NULL, NULL, 'Free plan with limited access', 0, 0.00),
    ('PRO', 'Pro Plan', 'prod_SMeKyTOkn9HmxO', 'price_1RRv2ECQhowdgEANcbQ3xr2s', 'Premium features', 5, 9.99),
    ('ENTERPRISE', 'Enterprise Plan', 'prod_SMeMuctQXhsOls', 'price_1RRv43CQhowdgEANT13Lza3v', 'Unlimited access', 20, 29.99);