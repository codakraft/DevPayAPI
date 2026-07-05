IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AdminSettings] (
        [Id] nvarchar(450) NOT NULL,
        [SettingKey] nvarchar(100) NOT NULL,
        [SettingValue] nvarchar(max) NOT NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_AdminSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AppLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [Timestamp] datetime2 NOT NULL,
        [Level] nvarchar(20) NOT NULL,
        [Category] nvarchar(512) NULL,
        [Message] nvarchar(max) NOT NULL,
        [Exception] nvarchar(max) NULL,
        [EventId] nvarchar(100) NULL,
        CONSTRAINT [PK_AppLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Address] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [CompanyId] nvarchar(max) NULL,
        [Gender] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [RequiresPasswordChange] bit NOT NULL,
        [DateOfBirth] datetime2 NULL,
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
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [Category] nvarchar(max) NOT NULL,
        [UserId] nvarchar(max) NULL,
        [UserEmail] nvarchar(max) NULL,
        [EntityType] nvarchar(max) NULL,
        [EntityId] nvarchar(max) NULL,
        [CompanyId] uniqueidentifier NULL,
        [Details] nvarchar(max) NULL,
        [Amount] decimal(18,2) NULL,
        [OldBalance] decimal(18,2) NULL,
        [NewBalance] decimal(18,2) NULL,
        [IpAddress] nvarchar(max) NULL,
        [IsSuccess] bit NOT NULL,
        [ErrorMessage] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ShortName] nvarchar(max) NOT NULL,
        [Street] nvarchar(max) NULL,
        [City] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [LogoDocumentId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Disbursements] (
        [Id] uniqueidentifier NOT NULL,
        [LoanId] nvarchar(max) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [AccountDetails] nvarchar(max) NOT NULL,
        [DisbursementMethod] nvarchar(max) NOT NULL,
        [RequestedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [ProcessedBy] nvarchar(max) NULL,
        [Notes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Disbursements] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Documents] (
        [Id] nvarchar(450) NOT NULL,
        [DocumentName] nvarchar(255) NOT NULL,
        [DocumentType] nvarchar(100) NULL,
        [DocumentUrl] nvarchar(500) NULL,
        [UploadedBy] nvarchar(255) NULL,
        [Status] int NOT NULL,
        [ErrorMessage] nvarchar(1000) NULL,
        [FileExtension] nvarchar(50) NULL,
        [FileSizeBytes] bigint NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UploadedAt] datetime2 NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(max) NOT NULL,
        [Employer] nvarchar(max) NOT NULL,
        [Industry] nvarchar(max) NOT NULL,
        [Role] nvarchar(max) NOT NULL,
        [ResidentialAddress] nvarchar(max) NOT NULL,
        [LoanId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [EWallets] (
        [Id] uniqueidentifier NOT NULL,
        [EmbedlyWalletId] nvarchar(100) NOT NULL,
        [EmbedlyCustomerId] nvarchar(100) NOT NULL,
        [WalletGroupId] nvarchar(100) NULL,
        [CurrencyId] nvarchar(100) NULL,
        [AccountNumber] nvarchar(20) NULL,
        [BankCode] nvarchar(20) NULL,
        [BankName] nvarchar(100) NULL,
        [AccountName] nvarchar(200) NULL,
        [MobileNumber] nvarchar(20) NULL,
        [AvailableBalance] decimal(18,2) NULL,
        [LedgerBalance] decimal(18,2) NULL,
        [IsDefault] bit NOT NULL,
        [Classification] nvarchar(50) NULL,
        [RawResponse] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EWallets] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [MfaSessions] (
        [SessionId] nvarchar(100) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MfaSessions] PRIMARY KEY ([SessionId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [MonoBvnVerificationRecords] (
        [Id] uniqueidentifier NOT NULL,
        [BvnHash] nvarchar(64) NOT NULL,
        [IsVerified] bit NOT NULL,
        [VerifiedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [Provider] nvarchar(10) NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [DateOfBirth] nvarchar(20) NOT NULL,
        [PhoneNumber] nvarchar(20) NOT NULL,
        [LastSessionId] nvarchar(100) NULL,
        [VerifiedByUserId] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MonoBvnVerificationRecords] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Otps] (
        [Id] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Purpose] nvarchar(max) NOT NULL,
        [RecipientIdentifier] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [CodeLength] int NOT NULL,
        [GeneratedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [ExpiryMinutes] int NOT NULL,
        [AttemptCount] int NOT NULL,
        [MaxAttempts] int NOT NULL,
        [IsLocked] bit NOT NULL,
        [LockedAt] datetime2 NULL,
        [IsUsed] bit NOT NULL,
        [UsedAt] datetime2 NULL,
        [IsInvalidated] bit NOT NULL,
        [InvalidatedAt] datetime2 NULL,
        [InvalidationReason] nvarchar(max) NULL,
        [DeliveryChannel] int NOT NULL,
        [WasDelivered] bit NOT NULL,
        [DeliveredAt] datetime2 NULL,
        [DeliveryError] nvarchar(max) NULL,
        [CompanyId] uniqueidentifier NULL,
        [RelatedEntityId] uniqueidentifier NULL,
        [RelatedEntityType] nvarchar(max) NULL,
        [CreatedBy] nvarchar(max) NULL,
        [IpAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Otps] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [RemitaLoanCollectionNotifications] (
        [Id] uniqueidentifier NOT NULL,
        [RemitaId] bigint NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [ModuleName] nvarchar(100) NULL,
        [NotificationSent] bit NOT NULL,
        [NetSalary] decimal(18,2) NULL,
        [TotalCredit] decimal(18,2) NULL,
        [MandateRef] nvarchar(100) NOT NULL,
        [BalanceDue] decimal(18,2) NULL,
        [CustomerId] nvarchar(100) NULL,
        [PaymentDate] datetime2 NULL,
        [PaymentStatus] nvarchar(50) NULL,
        [RawPayload] nvarchar(max) NULL,
        [Payload] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RemitaLoanCollectionNotifications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Settings] (
        [Id] uniqueidentifier NOT NULL,
        [LegalFee] decimal(18,2) NOT NULL,
        [MaintenanceFee] decimal(18,2) NOT NULL,
        [ProcessingFee] decimal(18,2) NOT NULL,
        [PenaltyFee] decimal(18,2) NOT NULL,
        [LateFee] decimal(18,2) NOT NULL,
        [OtpFee] decimal(18,2) NOT NULL,
        [DocumentationFee] decimal(18,2) NOT NULL,
        [OtpFeeType] int NOT NULL,
        [LegalFeeType] int NOT NULL,
        [MaintenanceFeeType] int NOT NULL,
        [ProcessingFeeType] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(450) NULL,
        [UpdatedBy] nvarchar(450) NULL,
        CONSTRAINT [PK_Settings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Token] nvarchar(450) NOT NULL,
        [ExpiryDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [IsRevoked] bit NOT NULL,
        [RevokedAt] datetime2 NULL,
        [RevokedByUserId] nvarchar(450) NULL,
        [ReasonRevoked] nvarchar(500) NULL,
        [ReplacedByToken] nvarchar(500) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_RevokedByUserId] FOREIGN KEY ([RevokedByUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Approvals] (
        [Id] nvarchar(450) NOT NULL,
        [ApprovalType] nvarchar(50) NOT NULL,
        [ReferenceId] nvarchar(450) NOT NULL,
        [RequestedBy] nvarchar(450) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [Reason] nvarchar(1000) NULL,
        [RequestedAt] datetime2 NOT NULL,
        [ProcessedAt] datetime2 NULL,
        [ProcessedBy] nvarchar(450) NULL,
        [CompanyId] uniqueidentifier NULL,
        CONSTRAINT [PK_Approvals] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Approvals_AspNetUsers_ProcessedBy] FOREIGN KEY ([ProcessedBy]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Approvals_AspNetUsers_RequestedBy] FOREIGN KEY ([RequestedBy]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Approvals_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [LoanProducts] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ShortName] nvarchar(max) NOT NULL,
        [MinAmount] decimal(18,2) NOT NULL,
        [MaxAmount] decimal(18,2) NOT NULL,
        [MinTenor] int NOT NULL,
        [MaxTenor] int NOT NULL,
        [InterestRate] decimal(5,4) NOT NULL,
        [PenaltyOnDefaultPrincipal] decimal(18,2) NOT NULL,
        [ProcessingFeePercent] decimal(18,2) NOT NULL,
        [ProcessingFeeFlat] decimal(18,2) NOT NULL,
        [MaintenanceFeePercent] decimal(18,2) NOT NULL,
        [LegalFeePercent] decimal(18,2) NOT NULL,
        [LegalFeeFlat] decimal(18,2) NOT NULL,
        [Moratorium] int NOT NULL,
        [NotifyApprovalsViaEmail] bit NOT NULL,
        [TurnoverEligibilityPercent] decimal(18,2) NOT NULL,
        [EligibilityPercentage] decimal(18,2) NOT NULL,
        [InterestComputationBasis] int NOT NULL,
        [InterestCostComputation] int NOT NULL,
        [PaymentScheduleBreakdown] int NOT NULL,
        [PaymentScheduleType] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LoanProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LoanProducts_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [SupportTickets] (
        [Id] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Subject] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [AssignedTo] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [ResolvedAt] datetime2 NULL,
        CONSTRAINT [PK_SupportTickets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportTickets_AspNetUsers_AssignedTo] FOREIGN KEY ([AssignedTo]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_SupportTickets_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_SupportTickets_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Wallets] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NULL,
        [Balance] decimal(18,2) NOT NULL,
        [TotalCredits] decimal(18,2) NOT NULL,
        [TotalDebits] decimal(18,2) NOT NULL,
        [IsSuperAdminWallet] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Wallets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Wallets_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Loans] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] nvarchar(450) NULL,
        [Amount] decimal(18,2) NOT NULL,
        [DurationInMonths] int NOT NULL,
        [Purpose] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [ApprovedAt] datetime2 NULL,
        [DueDate] datetime2 NULL,
        [RejectedAt] datetime2 NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovedByUserId] nvarchar(450) NULL,
        [RejectedBy] nvarchar(max) NULL,
        [RejectedByUserId] nvarchar(450) NULL,
        [Reason] nvarchar(max) NULL,
        [MandateCreatedAt] datetime2 NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [IsMandateCreated] bit NOT NULL,
        [MandateRef] nvarchar(max) NOT NULL,
        [DisbursementDate] datetime2 NULL,
        [DisbursementReference] nvarchar(max) NULL,
        [MandateStoppedDate] datetime2 NULL,
        [MandateStoppedAt] datetime2 NULL,
        [MandateStoppedBy] nvarchar(max) NULL,
        [OfferLetterDocumentId] uniqueidentifier NULL,
        [OfferLetterUrl] nvarchar(max) NULL,
        [OfferLetterSentAt] datetime2 NULL,
        [SignedOfferLetterDocumentId] uniqueidentifier NULL,
        [SignedOfferLetterUploadedAt] datetime2 NULL,
        [TotalRepayment] decimal(18,2) NULL,
        [MonthlyRepayment] decimal(18,2) NULL,
        [DisbursementAmount] decimal(18,2) NULL,
        [ApplicableFees] decimal(18,2) NULL,
        [AppliedInterest] decimal(18,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Loans] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Loans_AspNetUsers_ApprovedByUserId] FOREIGN KEY ([ApprovedByUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Loans_AspNetUsers_RejectedByUserId] FOREIGN KEY ([RejectedByUserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Loans_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_Loans_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
        CONSTRAINT [FK_Loans_LoanProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [LoanProducts] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [SupportComments] (
        [Id] nvarchar(450) NOT NULL,
        [TicketId] nvarchar(450) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Comment] nvarchar(1000) NOT NULL,
        [IsInternal] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SupportComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SupportComments_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_SupportComments_SupportTickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [SupportTickets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [WalletTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [WalletId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [BalanceAfter] decimal(18,2) NOT NULL,
        [TransactionType] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ReferenceId] nvarchar(max) NULL,
        [InitiatedBy] nvarchar(450) NULL,
        [PaystackReference] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WalletTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WalletTransactions_AspNetUsers_InitiatedBy] FOREIGN KEY ([InitiatedBy]) REFERENCES [AspNetUsers] ([Id]),
        CONSTRAINT [FK_WalletTransactions_Wallets_WalletId] FOREIGN KEY ([WalletId]) REFERENCES [Wallets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [BorrowerApplications] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [Employer] nvarchar(max) NOT NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [BankCode] nvarchar(max) NULL,
        [AccountNo] nvarchar(max) NULL,
        [BVN] nvarchar(max) NULL,
        [Address] nvarchar(max) NULL,
        [IdNumber] nvarchar(max) NULL,
        [DocumentIds] nvarchar(max) NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [ProductId] uniqueidentifier NOT NULL,
        [LoanId] uniqueidentifier NULL,
        [MaxLoanEligible] decimal(18,2) NULL,
        [MinLoanEligible] decimal(18,2) NULL,
        [MaxTenor] int NULL,
        [MinTenor] int NULL,
        [CurrentStep] int NOT NULL,
        [EmailVerifiedAt] datetime2 NULL,
        [BvnVerifiedAt] datetime2 NULL,
        [DocumentsUploadedAt] datetime2 NULL,
        [LoanSubmittedAt] datetime2 NULL,
        [LastEmailOtp] nvarchar(max) NULL,
        [EmailOtpGeneratedAt] datetime2 NULL,
        [LastBvnOtp] nvarchar(max) NULL,
        [BvnOtpGeneratedAt] datetime2 NULL,
        [MonoBvnSessionId] nvarchar(max) NULL,
        [MonoBvnMethod] nvarchar(max) NULL,
        [MonoBvnMethodHint] nvarchar(max) NULL,
        [IsBvnVerified] bit NOT NULL,
        [MonoBvnVerifiedData] nvarchar(max) NULL,
        [IsOfferLetterAccepted] bit NOT NULL,
        [OfferLetterAcceptedAt] datetime2 NULL,
        [DirectDebitMandateId] nvarchar(max) NULL,
        [RemitaTransRef] nvarchar(max) NULL,
        [MandateGeneratedAt] datetime2 NULL,
        [MandateActivatedAt] datetime2 NULL,
        [IsCompleted] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_BorrowerApplications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BorrowerApplications_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
        CONSTRAINT [FK_BorrowerApplications_LoanProducts_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [LoanProducts] ([Id]),
        CONSTRAINT [FK_BorrowerApplications_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [MonoMandateReferences] (
        [Id] uniqueidentifier NOT NULL,
        [CompanyId] uniqueidentifier NOT NULL,
        [LoanId] uniqueidentifier NOT NULL,
        [MandateId] nvarchar(max) NOT NULL,
        [Reference] nvarchar(max) NOT NULL,
        [NibssCode] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [MandateType] nvarchar(max) NOT NULL,
        [DebitType] nvarchar(max) NOT NULL,
        [ReadyToDebit] bit NOT NULL,
        [Approved] bit NOT NULL,
        [AccountName] nvarchar(max) NOT NULL,
        [AccountNumber] nvarchar(max) NOT NULL,
        [Bank] nvarchar(max) NOT NULL,
        [BankCode] nvarchar(max) NOT NULL,
        [Customer] nvarchar(max) NOT NULL,
        [FeeBearer] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [LiveMode] bit NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [InitialDebitDate] datetime2 NOT NULL,
        [Amount] int NOT NULL,
        [InitialDebitAmount] int NOT NULL,
        [TransferDestinationsJson] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MonoMandateReferences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MonoMandateReferences_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
        CONSTRAINT [FK_MonoMandateReferences_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [Repayments] (
        [Id] uniqueidentifier NOT NULL,
        [LoanId] uniqueidentifier NOT NULL,
        [TotalDue] decimal(18,2) NOT NULL,
        [TotalRepaid] decimal(18,2) NOT NULL,
        [LastPaymentAt] datetime2 NULL,
        [AmountUnpaid] decimal(18,2) NOT NULL,
        [Status] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Repayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Repayments_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [MonoCreditAnalysisRecords] (
        [Id] uniqueidentifier NOT NULL,
        [BorrowerApplicationId] uniqueidentifier NOT NULL,
        [BvnHash] nvarchar(64) NOT NULL,
        [Provider] nvarchar(10) NOT NULL,
        [CreditScore] decimal(18,2) NOT NULL,
        [MaxLoanAmount] decimal(18,2) NOT NULL,
        [RiskLevel] nvarchar(20) NOT NULL,
        [ActiveLoansCount] int NOT NULL,
        [TotalOutstandingDebt] decimal(18,2) NOT NULL,
        [OverallPerformanceStatus] nvarchar(50) NOT NULL,
        [RecommendedAction] nvarchar(20) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MonoCreditAnalysisRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MonoCreditAnalysisRecords_BorrowerApplications_BorrowerApplicationId] FOREIGN KEY ([BorrowerApplicationId]) REFERENCES [BorrowerApplications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [RemitaSalaryHistories] (
        [Id] uniqueidentifier NOT NULL,
        [BorrowerApplicationId] uniqueidentifier NOT NULL,
        [CustomerId] nvarchar(50) NOT NULL,
        [AuthorisationCode] nvarchar(20) NULL,
        [AccountNumber] nvarchar(20) NOT NULL,
        [BankCode] nvarchar(10) NOT NULL,
        [BVN] nvarchar(20) NULL,
        [CompanyName] nvarchar(200) NULL,
        [CustomerName] nvarchar(200) NULL,
        [Category] nvarchar(100) NULL,
        [FirstPaymentDate] datetime2 NULL,
        [SalaryCount] int NOT NULL,
        [AverageMonthlySalary] decimal(18,2) NOT NULL,
        [LatestSalaryAmount] decimal(18,2) NOT NULL,
        [LatestPaymentDate] datetime2 NULL,
        [MinSalaryAmount] decimal(18,2) NOT NULL,
        [MaxSalaryAmount] decimal(18,2) NOT NULL,
        [ConsistentMonths] int NOT NULL,
        [HasOutstandingLoans] bit NOT NULL,
        [TotalOutstandingAmount] decimal(18,2) NOT NULL,
        [RawRemitaResponse] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RemitaSalaryHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RemitaSalaryHistories_BorrowerApplications_BorrowerApplicationId] FOREIGN KEY ([BorrowerApplicationId]) REFERENCES [BorrowerApplications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE TABLE [RemitaSalaryPayments] (
        [Id] uniqueidentifier NOT NULL,
        [RemitaSalaryHistoryId] uniqueidentifier NOT NULL,
        [PaymentDate] datetime2 NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [AccountNumber] nvarchar(20) NOT NULL,
        [BankCode] nvarchar(10) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_RemitaSalaryPayments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RemitaSalaryPayments_RemitaSalaryHistories_RemitaSalaryHistoryId] FOREIGN KEY ([RemitaSalaryHistoryId]) REFERENCES [RemitaSalaryHistories] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AdminSettings_SettingKey] ON [AdminSettings] ([SettingKey]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Approvals_CompanyId] ON [Approvals] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Approvals_ProcessedBy] ON [Approvals] ([ProcessedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Approvals_RequestedBy] ON [Approvals] ([RequestedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_AspNetUsers_Email] ON [AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_BorrowerApplications_CompanyId] ON [BorrowerApplications] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_BorrowerApplications_CurrentStep] ON [BorrowerApplications] ([CurrentStep]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_BorrowerApplications_Email] ON [BorrowerApplications] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_BorrowerApplications_LoanId] ON [BorrowerApplications] ([LoanId]) WHERE [LoanId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_BorrowerApplications_ProductId] ON [BorrowerApplications] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_LoanProducts_CompanyId] ON [LoanProducts] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Loans_ApprovedByUserId] ON [Loans] ([ApprovedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Loans_CompanyId] ON [Loans] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Loans_ProductId] ON [Loans] ([ProductId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Loans_RejectedByUserId] ON [Loans] ([RejectedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Loans_UserId] ON [Loans] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_BvnHash] ON [MonoBvnVerificationRecords] ([BvnHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_CreatedAt] ON [MonoBvnVerificationRecords] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_ExpiresAt] ON [MonoBvnVerificationRecords] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_BorrowerApplicationId] ON [MonoCreditAnalysisRecords] ([BorrowerApplicationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_BvnHash] ON [MonoCreditAnalysisRecords] ([BvnHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_CreatedAt] ON [MonoCreditAnalysisRecords] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_ExpiresAt] ON [MonoCreditAnalysisRecords] ([ExpiresAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoMandateReferences_CompanyId] ON [MonoMandateReferences] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_MonoMandateReferences_LoanId] ON [MonoMandateReferences] ([LoanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_RevokedByUserId] ON [RefreshTokens] ([RevokedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RemitaSalaryHistories_AccountNumber_BankCode] ON [RemitaSalaryHistories] ([AccountNumber], [BankCode]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RemitaSalaryHistories_BorrowerApplicationId] ON [RemitaSalaryHistories] ([BorrowerApplicationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RemitaSalaryHistories_CustomerId] ON [RemitaSalaryHistories] ([CustomerId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RemitaSalaryPayments_PaymentDate] ON [RemitaSalaryPayments] ([PaymentDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_RemitaSalaryPayments_RemitaSalaryHistoryId] ON [RemitaSalaryPayments] ([RemitaSalaryHistoryId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Repayments_LoanId] ON [Repayments] ([LoanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportComments_TicketId] ON [SupportComments] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportComments_UserId] ON [SupportComments] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_AssignedTo] ON [SupportTickets] ([AssignedTo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_Category] ON [SupportTickets] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_CompanyId_Status] ON [SupportTickets] ([CompanyId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_CreatedAt] ON [SupportTickets] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_Priority] ON [SupportTickets] ([Priority]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_Status] ON [SupportTickets] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_SupportTickets_UserId_Status] ON [SupportTickets] ([UserId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Wallets_CompanyId] ON [Wallets] ([CompanyId]) WHERE [CompanyId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_Wallets_IsSuperAdminWallet] ON [Wallets] ([IsSuperAdminWallet]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_WalletTransactions_CreatedAt] ON [WalletTransactions] ([CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_WalletTransactions_InitiatedBy] ON [WalletTransactions] ([InitiatedBy]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_WalletTransactions_PaystackReference] ON [WalletTransactions] ([PaystackReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_WalletTransactions_TransactionType] ON [WalletTransactions] ([TransactionType]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    CREATE INDEX [IX_WalletTransactions_WalletId] ON [WalletTransactions] ([WalletId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529112519_AddEWalletsTable', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','PassportUrl') IS NULL ALTER TABLE [EWallets] ADD [PassportUrl] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EWallets') AND name = 'IsDefault' AND is_nullable = 0) ALTER TABLE [EWallets] ALTER COLUMN [IsDefault] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EWallets') AND name = 'EmbedlyWalletId' AND is_nullable = 0) ALTER TABLE [EWallets] ALTER COLUMN [EmbedlyWalletId] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Address') IS NULL ALTER TABLE [EWallets] ADD [Address] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Alias') IS NULL ALTER TABLE [EWallets] ADD [Alias] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Bvn') IS NULL ALTER TABLE [EWallets] ADD [Bvn] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','BvnVerified') IS NULL ALTER TABLE [EWallets] ADD [BvnVerified] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','City') IS NULL ALTER TABLE [EWallets] ADD [City] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','CustomerRawResponse') IS NULL ALTER TABLE [EWallets] ADD [CustomerRawResponse] nvarchar(4000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Dob') IS NULL ALTER TABLE [EWallets] ADD [Dob] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','EmailAddress') IS NULL ALTER TABLE [EWallets] ADD [EmailAddress] nvarchar(256) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','FirstName') IS NULL ALTER TABLE [EWallets] ADD [FirstName] nvarchar(100) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Gender') IS NULL ALTER TABLE [EWallets] ADD [Gender] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','KycTier') IS NULL ALTER TABLE [EWallets] ADD [KycTier] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','LastName') IS NULL ALTER TABLE [EWallets] ADD [LastName] nvarchar(100) NOT NULL DEFAULT '';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','MaritalStatus') IS NULL ALTER TABLE [EWallets] ADD [MaritalStatus] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','MiddleName') IS NULL ALTER TABLE [EWallets] ADD [MiddleName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','MothersMaidenName') IS NULL ALTER TABLE [EWallets] ADD [MothersMaidenName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinAddress') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinAddress] nvarchar(200) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinFirstName') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinFirstName] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinMobileNumber') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinMobileNumber] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinOtherNames') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinOtherNames] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinRelationship') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinRelationship] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NextOfKinSurname') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinSurname] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Nin') IS NULL ALTER TABLE [EWallets] ADD [Nin] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','NinVerified') IS NULL ALTER TABLE [EWallets] ADD [NinVerified] bit NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','Occupation') IS NULL ALTER TABLE [EWallets] ADD [Occupation] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    IF COL_LENGTH('EWallets','WalletRawResponse') IS NULL ALTER TABLE [EWallets] ADD [WalletRawResponse] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530074251_AddEWalletKycColumns', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530183858_AddEWalletTransactionsTable'
)
BEGIN
    CREATE TABLE [EWalletTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [TransactionReference] nvarchar(100) NOT NULL,
        [FromAccount] nvarchar(20) NOT NULL,
        [ToAccount] nvarchar(20) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Remarks] nvarchar(500) NULL,
        [Status] nvarchar(50) NOT NULL,
        [RawResponse] nvarchar(4000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_EWalletTransactions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530183858_AddEWalletTransactionsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530183858_AddEWalletTransactionsTable', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617141941_AddMonoCustomerIdToBorrowerApplication'
)
BEGIN
    ALTER TABLE [BorrowerApplications] ADD [MonoCustomerId] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617141941_AddMonoCustomerIdToBorrowerApplication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617141941_AddMonoCustomerIdToBorrowerApplication', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618092329_AddMonoMandateReferencesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260618092329_AddMonoMandateReferencesTable', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618094850_CreateMonoMandateReferencesTableFix'
)
BEGIN

                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MonoMandateReferences')
                    BEGIN
                        CREATE TABLE [MonoMandateReferences] (
                            [Id] uniqueidentifier NOT NULL,
                            [CompanyId] uniqueidentifier NOT NULL,
                            [LoanId] uniqueidentifier NOT NULL,
                            [MandateId] nvarchar(max) NOT NULL,
                            [Reference] nvarchar(max) NOT NULL,
                            [NibssCode] nvarchar(max) NOT NULL,
                            [Status] nvarchar(max) NOT NULL,
                            [MandateType] nvarchar(max) NOT NULL,
                            [DebitType] nvarchar(max) NOT NULL,
                            [ReadyToDebit] bit NOT NULL DEFAULT 0,
                            [Approved] bit NOT NULL DEFAULT 0,
                            [AccountName] nvarchar(max) NOT NULL,
                            [AccountNumber] nvarchar(max) NOT NULL,
                            [Bank] nvarchar(max) NOT NULL,
                            [BankCode] nvarchar(max) NOT NULL,
                            [Customer] nvarchar(max) NOT NULL,
                            [FeeBearer] nvarchar(max) NOT NULL,
                            [Description] nvarchar(max) NOT NULL,
                            [LiveMode] bit NOT NULL DEFAULT 0,
                            [StartDate] datetime2 NOT NULL,
                            [EndDate] datetime2 NOT NULL,
                            [InitialDebitDate] datetime2 NOT NULL,
                            [Amount] int NOT NULL DEFAULT 0,
                            [InitialDebitAmount] int NOT NULL DEFAULT 0,
                            [TransferDestinationsJson] nvarchar(max) NOT NULL,
                            [CreatedAt] datetime2 NOT NULL,
                            [UpdatedAt] datetime2 NOT NULL,
                            CONSTRAINT [PK_MonoMandateReferences] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_MonoMandateReferences_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]),
                            CONSTRAINT [FK_MonoMandateReferences_Loans_LoanId] FOREIGN KEY ([LoanId]) REFERENCES [Loans] ([Id])
                        );

                        CREATE INDEX [IX_MonoMandateReferences_CompanyId] ON [MonoMandateReferences] ([CompanyId]);
                        CREATE INDEX [IX_MonoMandateReferences_LoanId] ON [MonoMandateReferences] ([LoanId]);
                    END
                
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260618094850_CreateMonoMandateReferencesTableFix'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260618094850_CreateMonoMandateReferencesTableFix', N'9.0.6');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705093918_AddCreditAnalysisFactorsJson'
)
BEGIN
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [PositiveFactorsJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705093918_AddCreditAnalysisFactorsJson'
)
BEGIN
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [RiskFactorsJson] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260705093918_AddCreditAnalysisFactorsJson'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705093918_AddCreditAnalysisFactorsJson', N'9.0.6');
END;

COMMIT;
GO

