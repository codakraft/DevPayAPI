-- Column reconciliation: adds any column the dev EF model expects that prod is missing.
-- Every ADD is guarded by IF COL_LENGTH(...) IS NULL, so already-present columns are skipped.
-- NOT NULL columns get a type-appropriate DEFAULT so existing rows backfill safely.
-- Idempotent and safe to re-run. Wrapped in a transaction.

BEGIN TRANSACTION;
IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','Id') IS NULL
    ALTER TABLE [AdminSettings] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','SettingKey') IS NULL
    ALTER TABLE [AdminSettings] ADD [SettingKey] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','SettingValue') IS NULL
    ALTER TABLE [AdminSettings] ADD [SettingValue] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','Description') IS NULL
    ALTER TABLE [AdminSettings] ADD [Description] nvarchar(500) NULL;

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','IsActive') IS NULL
    ALTER TABLE [AdminSettings] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','CreatedAt') IS NULL
    ALTER TABLE [AdminSettings] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','UpdatedAt') IS NULL
    ALTER TABLE [AdminSettings] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','CreatedBy') IS NULL
    ALTER TABLE [AdminSettings] ADD [CreatedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[AdminSettings]', N'U') IS NOT NULL AND COL_LENGTH('AdminSettings','UpdatedBy') IS NULL
    ALTER TABLE [AdminSettings] ADD [UpdatedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','Timestamp') IS NULL
    ALTER TABLE [AppLogs] ADD [Timestamp] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','Level') IS NULL
    ALTER TABLE [AppLogs] ADD [Level] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','Category') IS NULL
    ALTER TABLE [AppLogs] ADD [Category] nvarchar(512) NULL;

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','Message') IS NULL
    ALTER TABLE [AppLogs] ADD [Message] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','Exception') IS NULL
    ALTER TABLE [AppLogs] ADD [Exception] nvarchar(max) NULL;

IF OBJECT_ID(N'[AppLogs]', N'U') IS NOT NULL AND COL_LENGTH('AppLogs','EventId') IS NULL
    ALTER TABLE [AppLogs] ADD [EventId] nvarchar(100) NULL;

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoles','Id') IS NULL
    ALTER TABLE [AspNetRoles] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoles','Name') IS NULL
    ALTER TABLE [AspNetRoles] ADD [Name] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoles','NormalizedName') IS NULL
    ALTER TABLE [AspNetRoles] ADD [NormalizedName] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoles','ConcurrencyStamp') IS NULL
    ALTER TABLE [AspNetRoles] ADD [ConcurrencyStamp] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','Id') IS NULL
    ALTER TABLE [AspNetUsers] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','FirstName') IS NULL
    ALTER TABLE [AspNetUsers] ADD [FirstName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','LastName') IS NULL
    ALTER TABLE [AspNetUsers] ADD [LastName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','Address') IS NULL
    ALTER TABLE [AspNetUsers] ADD [Address] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','City') IS NULL
    ALTER TABLE [AspNetUsers] ADD [City] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','State') IS NULL
    ALTER TABLE [AspNetUsers] ADD [State] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','CompanyId') IS NULL
    ALTER TABLE [AspNetUsers] ADD [CompanyId] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','Gender') IS NULL
    ALTER TABLE [AspNetUsers] ADD [Gender] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','CreatedAt') IS NULL
    ALTER TABLE [AspNetUsers] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','IsActive') IS NULL
    ALTER TABLE [AspNetUsers] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','LastLoginAt') IS NULL
    ALTER TABLE [AspNetUsers] ADD [LastLoginAt] datetime2 NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','RequiresPasswordChange') IS NULL
    ALTER TABLE [AspNetUsers] ADD [RequiresPasswordChange] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','DateOfBirth') IS NULL
    ALTER TABLE [AspNetUsers] ADD [DateOfBirth] datetime2 NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','UserName') IS NULL
    ALTER TABLE [AspNetUsers] ADD [UserName] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','NormalizedUserName') IS NULL
    ALTER TABLE [AspNetUsers] ADD [NormalizedUserName] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','Email') IS NULL
    ALTER TABLE [AspNetUsers] ADD [Email] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','NormalizedEmail') IS NULL
    ALTER TABLE [AspNetUsers] ADD [NormalizedEmail] nvarchar(256) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','EmailConfirmed') IS NULL
    ALTER TABLE [AspNetUsers] ADD [EmailConfirmed] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','PasswordHash') IS NULL
    ALTER TABLE [AspNetUsers] ADD [PasswordHash] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','SecurityStamp') IS NULL
    ALTER TABLE [AspNetUsers] ADD [SecurityStamp] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','ConcurrencyStamp') IS NULL
    ALTER TABLE [AspNetUsers] ADD [ConcurrencyStamp] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','PhoneNumber') IS NULL
    ALTER TABLE [AspNetUsers] ADD [PhoneNumber] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','PhoneNumberConfirmed') IS NULL
    ALTER TABLE [AspNetUsers] ADD [PhoneNumberConfirmed] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','TwoFactorEnabled') IS NULL
    ALTER TABLE [AspNetUsers] ADD [TwoFactorEnabled] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','LockoutEnd') IS NULL
    ALTER TABLE [AspNetUsers] ADD [LockoutEnd] datetimeoffset NULL;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','LockoutEnabled') IS NULL
    ALTER TABLE [AspNetUsers] ADD [LockoutEnabled] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUsers','AccessFailedCount') IS NULL
    ALTER TABLE [AspNetUsers] ADD [AccessFailedCount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Id') IS NULL
    ALTER TABLE [AuditLogs] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Timestamp') IS NULL
    ALTER TABLE [AuditLogs] ADD [Timestamp] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Action') IS NULL
    ALTER TABLE [AuditLogs] ADD [Action] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Category') IS NULL
    ALTER TABLE [AuditLogs] ADD [Category] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','UserId') IS NULL
    ALTER TABLE [AuditLogs] ADD [UserId] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','UserEmail') IS NULL
    ALTER TABLE [AuditLogs] ADD [UserEmail] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','EntityType') IS NULL
    ALTER TABLE [AuditLogs] ADD [EntityType] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','EntityId') IS NULL
    ALTER TABLE [AuditLogs] ADD [EntityId] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','CompanyId') IS NULL
    ALTER TABLE [AuditLogs] ADD [CompanyId] uniqueidentifier NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Details') IS NULL
    ALTER TABLE [AuditLogs] ADD [Details] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','Amount') IS NULL
    ALTER TABLE [AuditLogs] ADD [Amount] decimal(18,2) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','OldBalance') IS NULL
    ALTER TABLE [AuditLogs] ADD [OldBalance] decimal(18,2) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','NewBalance') IS NULL
    ALTER TABLE [AuditLogs] ADD [NewBalance] decimal(18,2) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','IpAddress') IS NULL
    ALTER TABLE [AuditLogs] ADD [IpAddress] nvarchar(max) NULL;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','IsSuccess') IS NULL
    ALTER TABLE [AuditLogs] ADD [IsSuccess] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','ErrorMessage') IS NULL
    ALTER TABLE [AuditLogs] ADD [ErrorMessage] nvarchar(max) NULL;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','Id') IS NULL
    ALTER TABLE [Companies] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','Name') IS NULL
    ALTER TABLE [Companies] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','ShortName') IS NULL
    ALTER TABLE [Companies] ADD [ShortName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','Street') IS NULL
    ALTER TABLE [Companies] ADD [Street] nvarchar(max) NULL;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','City') IS NULL
    ALTER TABLE [Companies] ADD [City] nvarchar(max) NULL;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','State') IS NULL
    ALTER TABLE [Companies] ADD [State] nvarchar(max) NULL;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','IsActive') IS NULL
    ALTER TABLE [Companies] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','LogoDocumentId') IS NULL
    ALTER TABLE [Companies] ADD [LogoDocumentId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','CreatedAt') IS NULL
    ALTER TABLE [Companies] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Companies]', N'U') IS NOT NULL AND COL_LENGTH('Companies','UpdatedAt') IS NULL
    ALTER TABLE [Companies] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','Id') IS NULL
    ALTER TABLE [Disbursements] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','LoanId') IS NULL
    ALTER TABLE [Disbursements] ADD [LoanId] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','Amount') IS NULL
    ALTER TABLE [Disbursements] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','Status') IS NULL
    ALTER TABLE [Disbursements] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','AccountDetails') IS NULL
    ALTER TABLE [Disbursements] ADD [AccountDetails] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','DisbursementMethod') IS NULL
    ALTER TABLE [Disbursements] ADD [DisbursementMethod] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','RequestedAt') IS NULL
    ALTER TABLE [Disbursements] ADD [RequestedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','ProcessedAt') IS NULL
    ALTER TABLE [Disbursements] ADD [ProcessedAt] datetime2 NULL;

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','ProcessedBy') IS NULL
    ALTER TABLE [Disbursements] ADD [ProcessedBy] nvarchar(max) NULL;

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','Notes') IS NULL
    ALTER TABLE [Disbursements] ADD [Notes] nvarchar(max) NULL;

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','CreatedAt') IS NULL
    ALTER TABLE [Disbursements] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Disbursements]', N'U') IS NOT NULL AND COL_LENGTH('Disbursements','UpdatedAt') IS NULL
    ALTER TABLE [Disbursements] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','Id') IS NULL
    ALTER TABLE [Documents] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','DocumentName') IS NULL
    ALTER TABLE [Documents] ADD [DocumentName] nvarchar(255) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','DocumentType') IS NULL
    ALTER TABLE [Documents] ADD [DocumentType] nvarchar(100) NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','DocumentUrl') IS NULL
    ALTER TABLE [Documents] ADD [DocumentUrl] nvarchar(500) NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','UploadedBy') IS NULL
    ALTER TABLE [Documents] ADD [UploadedBy] nvarchar(255) NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','Status') IS NULL
    ALTER TABLE [Documents] ADD [Status] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','ErrorMessage') IS NULL
    ALTER TABLE [Documents] ADD [ErrorMessage] nvarchar(1000) NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','FileExtension') IS NULL
    ALTER TABLE [Documents] ADD [FileExtension] nvarchar(50) NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','FileSizeBytes') IS NULL
    ALTER TABLE [Documents] ADD [FileSizeBytes] bigint NULL;

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','CreatedAt') IS NULL
    ALTER TABLE [Documents] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Documents]', N'U') IS NOT NULL AND COL_LENGTH('Documents','UploadedAt') IS NULL
    ALTER TABLE [Documents] ADD [UploadedAt] datetime2 NULL;

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','Id') IS NULL
    ALTER TABLE [Employees] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','UserId') IS NULL
    ALTER TABLE [Employees] ADD [UserId] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','Employer') IS NULL
    ALTER TABLE [Employees] ADD [Employer] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','Industry') IS NULL
    ALTER TABLE [Employees] ADD [Industry] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','Role') IS NULL
    ALTER TABLE [Employees] ADD [Role] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','ResidentialAddress') IS NULL
    ALTER TABLE [Employees] ADD [ResidentialAddress] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','LoanId') IS NULL
    ALTER TABLE [Employees] ADD [LoanId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','CreatedAt') IS NULL
    ALTER TABLE [Employees] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Employees]', N'U') IS NOT NULL AND COL_LENGTH('Employees','UpdatedAt') IS NULL
    ALTER TABLE [Employees] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Id') IS NULL
    ALTER TABLE [EWallets] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','EmbedlyWalletId') IS NULL
    ALTER TABLE [EWallets] ADD [EmbedlyWalletId] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','EmbedlyCustomerId') IS NULL
    ALTER TABLE [EWallets] ADD [EmbedlyCustomerId] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','WalletGroupId') IS NULL
    ALTER TABLE [EWallets] ADD [WalletGroupId] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','CurrencyId') IS NULL
    ALTER TABLE [EWallets] ADD [CurrencyId] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','AccountNumber') IS NULL
    ALTER TABLE [EWallets] ADD [AccountNumber] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','BankCode') IS NULL
    ALTER TABLE [EWallets] ADD [BankCode] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','BankName') IS NULL
    ALTER TABLE [EWallets] ADD [BankName] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','AccountName') IS NULL
    ALTER TABLE [EWallets] ADD [AccountName] nvarchar(200) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','MobileNumber') IS NULL
    ALTER TABLE [EWallets] ADD [MobileNumber] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','AvailableBalance') IS NULL
    ALTER TABLE [EWallets] ADD [AvailableBalance] decimal(18,2) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','LedgerBalance') IS NULL
    ALTER TABLE [EWallets] ADD [LedgerBalance] decimal(18,2) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','IsDefault') IS NULL
    ALTER TABLE [EWallets] ADD [IsDefault] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Classification') IS NULL
    ALTER TABLE [EWallets] ADD [Classification] nvarchar(50) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','RawResponse') IS NULL
    ALTER TABLE [EWallets] ADD [RawResponse] nvarchar(500) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','CreatedAt') IS NULL
    ALTER TABLE [EWallets] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','UpdatedAt') IS NULL
    ALTER TABLE [EWallets] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MfaSessions]', N'U') IS NOT NULL AND COL_LENGTH('MfaSessions','SessionId') IS NULL
    ALTER TABLE [MfaSessions] ADD [SessionId] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MfaSessions]', N'U') IS NOT NULL AND COL_LENGTH('MfaSessions','UserId') IS NULL
    ALTER TABLE [MfaSessions] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MfaSessions]', N'U') IS NOT NULL AND COL_LENGTH('MfaSessions','Email') IS NULL
    ALTER TABLE [MfaSessions] ADD [Email] nvarchar(256) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MfaSessions]', N'U') IS NOT NULL AND COL_LENGTH('MfaSessions','CreatedAt') IS NULL
    ALTER TABLE [MfaSessions] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MfaSessions]', N'U') IS NOT NULL AND COL_LENGTH('MfaSessions','ExpiresAt') IS NULL
    ALTER TABLE [MfaSessions] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','Id') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','BvnHash') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [BvnHash] nvarchar(64) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','IsVerified') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [IsVerified] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','VerifiedAt') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [VerifiedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','ExpiresAt') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','Provider') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [Provider] nvarchar(10) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','FullName') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [FullName] nvarchar(200) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','DateOfBirth') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [DateOfBirth] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','PhoneNumber') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [PhoneNumber] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','LastSessionId') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [LastSessionId] nvarchar(100) NULL;

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','VerifiedByUserId') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [VerifiedByUserId] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','CreatedAt') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoBvnVerificationRecords','UpdatedAt') IS NULL
    ALTER TABLE [MonoBvnVerificationRecords] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','Id') IS NULL
    ALTER TABLE [Otps] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','Type') IS NULL
    ALTER TABLE [Otps] ADD [Type] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','Purpose') IS NULL
    ALTER TABLE [Otps] ADD [Purpose] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','RecipientIdentifier') IS NULL
    ALTER TABLE [Otps] ADD [RecipientIdentifier] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','Code') IS NULL
    ALTER TABLE [Otps] ADD [Code] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','CodeLength') IS NULL
    ALTER TABLE [Otps] ADD [CodeLength] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','GeneratedAt') IS NULL
    ALTER TABLE [Otps] ADD [GeneratedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','ExpiresAt') IS NULL
    ALTER TABLE [Otps] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','ExpiryMinutes') IS NULL
    ALTER TABLE [Otps] ADD [ExpiryMinutes] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','AttemptCount') IS NULL
    ALTER TABLE [Otps] ADD [AttemptCount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','MaxAttempts') IS NULL
    ALTER TABLE [Otps] ADD [MaxAttempts] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','IsLocked') IS NULL
    ALTER TABLE [Otps] ADD [IsLocked] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','LockedAt') IS NULL
    ALTER TABLE [Otps] ADD [LockedAt] datetime2 NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','IsUsed') IS NULL
    ALTER TABLE [Otps] ADD [IsUsed] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','UsedAt') IS NULL
    ALTER TABLE [Otps] ADD [UsedAt] datetime2 NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','IsInvalidated') IS NULL
    ALTER TABLE [Otps] ADD [IsInvalidated] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','InvalidatedAt') IS NULL
    ALTER TABLE [Otps] ADD [InvalidatedAt] datetime2 NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','InvalidationReason') IS NULL
    ALTER TABLE [Otps] ADD [InvalidationReason] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','DeliveryChannel') IS NULL
    ALTER TABLE [Otps] ADD [DeliveryChannel] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','WasDelivered') IS NULL
    ALTER TABLE [Otps] ADD [WasDelivered] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','DeliveredAt') IS NULL
    ALTER TABLE [Otps] ADD [DeliveredAt] datetime2 NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','DeliveryError') IS NULL
    ALTER TABLE [Otps] ADD [DeliveryError] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','CompanyId') IS NULL
    ALTER TABLE [Otps] ADD [CompanyId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','RelatedEntityId') IS NULL
    ALTER TABLE [Otps] ADD [RelatedEntityId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','RelatedEntityType') IS NULL
    ALTER TABLE [Otps] ADD [RelatedEntityType] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','CreatedBy') IS NULL
    ALTER TABLE [Otps] ADD [CreatedBy] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','IpAddress') IS NULL
    ALTER TABLE [Otps] ADD [IpAddress] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','UserAgent') IS NULL
    ALTER TABLE [Otps] ADD [UserAgent] nvarchar(max) NULL;

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','CreatedAt') IS NULL
    ALTER TABLE [Otps] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Otps]', N'U') IS NOT NULL AND COL_LENGTH('Otps','UpdatedAt') IS NULL
    ALTER TABLE [Otps] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','Id') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','RemitaId') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [RemitaId] bigint NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','Amount') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','ModuleName') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [ModuleName] nvarchar(100) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','NotificationSent') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [NotificationSent] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','NetSalary') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [NetSalary] decimal(18,2) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','TotalCredit') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [TotalCredit] decimal(18,2) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','MandateRef') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [MandateRef] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','BalanceDue') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [BalanceDue] decimal(18,2) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','CustomerId') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [CustomerId] nvarchar(100) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','PaymentDate') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [PaymentDate] datetime2 NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','PaymentStatus') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [PaymentStatus] nvarchar(50) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','RawPayload') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [RawPayload] nvarchar(max) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','Payload') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [Payload] nvarchar(max) NULL;

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','CreatedAt') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaLoanCollectionNotifications]', N'U') IS NOT NULL AND COL_LENGTH('RemitaLoanCollectionNotifications','UpdatedAt') IS NULL
    ALTER TABLE [RemitaLoanCollectionNotifications] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','Id') IS NULL
    ALTER TABLE [Settings] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','LegalFee') IS NULL
    ALTER TABLE [Settings] ADD [LegalFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','MaintenanceFee') IS NULL
    ALTER TABLE [Settings] ADD [MaintenanceFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','ProcessingFee') IS NULL
    ALTER TABLE [Settings] ADD [ProcessingFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','PenaltyFee') IS NULL
    ALTER TABLE [Settings] ADD [PenaltyFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','LateFee') IS NULL
    ALTER TABLE [Settings] ADD [LateFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','OtpFee') IS NULL
    ALTER TABLE [Settings] ADD [OtpFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','DocumentationFee') IS NULL
    ALTER TABLE [Settings] ADD [DocumentationFee] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','OtpFeeType') IS NULL
    ALTER TABLE [Settings] ADD [OtpFeeType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','LegalFeeType') IS NULL
    ALTER TABLE [Settings] ADD [LegalFeeType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','MaintenanceFeeType') IS NULL
    ALTER TABLE [Settings] ADD [MaintenanceFeeType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','ProcessingFeeType') IS NULL
    ALTER TABLE [Settings] ADD [ProcessingFeeType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','CreatedAt') IS NULL
    ALTER TABLE [Settings] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','UpdatedAt') IS NULL
    ALTER TABLE [Settings] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','CreatedBy') IS NULL
    ALTER TABLE [Settings] ADD [CreatedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[Settings]', N'U') IS NOT NULL AND COL_LENGTH('Settings','UpdatedBy') IS NULL
    ALTER TABLE [Settings] ADD [UpdatedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoleClaims','RoleId') IS NULL
    ALTER TABLE [AspNetRoleClaims] ADD [RoleId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoleClaims','ClaimType') IS NULL
    ALTER TABLE [AspNetRoleClaims] ADD [ClaimType] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetRoleClaims','ClaimValue') IS NULL
    ALTER TABLE [AspNetRoleClaims] ADD [ClaimValue] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserClaims','UserId') IS NULL
    ALTER TABLE [AspNetUserClaims] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserClaims','ClaimType') IS NULL
    ALTER TABLE [AspNetUserClaims] ADD [ClaimType] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserClaims','ClaimValue') IS NULL
    ALTER TABLE [AspNetUserClaims] ADD [ClaimValue] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserLogins','LoginProvider') IS NULL
    ALTER TABLE [AspNetUserLogins] ADD [LoginProvider] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserLogins','ProviderKey') IS NULL
    ALTER TABLE [AspNetUserLogins] ADD [ProviderKey] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserLogins','ProviderDisplayName') IS NULL
    ALTER TABLE [AspNetUserLogins] ADD [ProviderDisplayName] nvarchar(max) NULL;

IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserLogins','UserId') IS NULL
    ALTER TABLE [AspNetUserLogins] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserRoles','UserId') IS NULL
    ALTER TABLE [AspNetUserRoles] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserRoles','RoleId') IS NULL
    ALTER TABLE [AspNetUserRoles] ADD [RoleId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserTokens','UserId') IS NULL
    ALTER TABLE [AspNetUserTokens] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserTokens','LoginProvider') IS NULL
    ALTER TABLE [AspNetUserTokens] ADD [LoginProvider] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserTokens','Name') IS NULL
    ALTER TABLE [AspNetUserTokens] ADD [Name] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NOT NULL AND COL_LENGTH('AspNetUserTokens','Value') IS NULL
    ALTER TABLE [AspNetUserTokens] ADD [Value] nvarchar(max) NULL;

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','Id') IS NULL
    ALTER TABLE [RefreshTokens] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','UserId') IS NULL
    ALTER TABLE [RefreshTokens] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','Token') IS NULL
    ALTER TABLE [RefreshTokens] ADD [Token] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','ExpiryDate') IS NULL
    ALTER TABLE [RefreshTokens] ADD [ExpiryDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','CreatedAt') IS NULL
    ALTER TABLE [RefreshTokens] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','IsRevoked') IS NULL
    ALTER TABLE [RefreshTokens] ADD [IsRevoked] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','RevokedAt') IS NULL
    ALTER TABLE [RefreshTokens] ADD [RevokedAt] datetime2 NULL;

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','RevokedByUserId') IS NULL
    ALTER TABLE [RefreshTokens] ADD [RevokedByUserId] nvarchar(450) NULL;

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','ReasonRevoked') IS NULL
    ALTER TABLE [RefreshTokens] ADD [ReasonRevoked] nvarchar(500) NULL;

IF OBJECT_ID(N'[RefreshTokens]', N'U') IS NOT NULL AND COL_LENGTH('RefreshTokens','ReplacedByToken') IS NULL
    ALTER TABLE [RefreshTokens] ADD [ReplacedByToken] nvarchar(500) NULL;

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','Id') IS NULL
    ALTER TABLE [Approvals] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','ApprovalType') IS NULL
    ALTER TABLE [Approvals] ADD [ApprovalType] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','ReferenceId') IS NULL
    ALTER TABLE [Approvals] ADD [ReferenceId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','RequestedBy') IS NULL
    ALTER TABLE [Approvals] ADD [RequestedBy] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','Status') IS NULL
    ALTER TABLE [Approvals] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','Description') IS NULL
    ALTER TABLE [Approvals] ADD [Description] nvarchar(1000) NULL;

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','Reason') IS NULL
    ALTER TABLE [Approvals] ADD [Reason] nvarchar(1000) NULL;

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','RequestedAt') IS NULL
    ALTER TABLE [Approvals] ADD [RequestedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','ProcessedAt') IS NULL
    ALTER TABLE [Approvals] ADD [ProcessedAt] datetime2 NULL;

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','ProcessedBy') IS NULL
    ALTER TABLE [Approvals] ADD [ProcessedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[Approvals]', N'U') IS NOT NULL AND COL_LENGTH('Approvals','CompanyId') IS NULL
    ALTER TABLE [Approvals] ADD [CompanyId] uniqueidentifier NULL;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','Id') IS NULL
    ALTER TABLE [LoanProducts] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','CompanyId') IS NULL
    ALTER TABLE [LoanProducts] ADD [CompanyId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','Name') IS NULL
    ALTER TABLE [LoanProducts] ADD [Name] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','Code') IS NULL
    ALTER TABLE [LoanProducts] ADD [Code] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','Description') IS NULL
    ALTER TABLE [LoanProducts] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','ShortName') IS NULL
    ALTER TABLE [LoanProducts] ADD [ShortName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','MinAmount') IS NULL
    ALTER TABLE [LoanProducts] ADD [MinAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','MaxAmount') IS NULL
    ALTER TABLE [LoanProducts] ADD [MaxAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','MinTenor') IS NULL
    ALTER TABLE [LoanProducts] ADD [MinTenor] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','MaxTenor') IS NULL
    ALTER TABLE [LoanProducts] ADD [MaxTenor] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','InterestRate') IS NULL
    ALTER TABLE [LoanProducts] ADD [InterestRate] decimal(5,4) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','PenaltyOnDefaultPrincipal') IS NULL
    ALTER TABLE [LoanProducts] ADD [PenaltyOnDefaultPrincipal] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','ProcessingFeePercent') IS NULL
    ALTER TABLE [LoanProducts] ADD [ProcessingFeePercent] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','ProcessingFeeFlat') IS NULL
    ALTER TABLE [LoanProducts] ADD [ProcessingFeeFlat] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','MaintenanceFeePercent') IS NULL
    ALTER TABLE [LoanProducts] ADD [MaintenanceFeePercent] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','LegalFeePercent') IS NULL
    ALTER TABLE [LoanProducts] ADD [LegalFeePercent] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','LegalFeeFlat') IS NULL
    ALTER TABLE [LoanProducts] ADD [LegalFeeFlat] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','Moratorium') IS NULL
    ALTER TABLE [LoanProducts] ADD [Moratorium] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','NotifyApprovalsViaEmail') IS NULL
    ALTER TABLE [LoanProducts] ADD [NotifyApprovalsViaEmail] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','TurnoverEligibilityPercent') IS NULL
    ALTER TABLE [LoanProducts] ADD [TurnoverEligibilityPercent] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','EligibilityPercentage') IS NULL
    ALTER TABLE [LoanProducts] ADD [EligibilityPercentage] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','InterestComputationBasis') IS NULL
    ALTER TABLE [LoanProducts] ADD [InterestComputationBasis] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','InterestCostComputation') IS NULL
    ALTER TABLE [LoanProducts] ADD [InterestCostComputation] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','PaymentScheduleBreakdown') IS NULL
    ALTER TABLE [LoanProducts] ADD [PaymentScheduleBreakdown] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','PaymentScheduleType') IS NULL
    ALTER TABLE [LoanProducts] ADD [PaymentScheduleType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','IsActive') IS NULL
    ALTER TABLE [LoanProducts] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','CreatedAt') IS NULL
    ALTER TABLE [LoanProducts] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[LoanProducts]', N'U') IS NOT NULL AND COL_LENGTH('LoanProducts','UpdatedAt') IS NULL
    ALTER TABLE [LoanProducts] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Id') IS NULL
    ALTER TABLE [SupportTickets] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','UserId') IS NULL
    ALTER TABLE [SupportTickets] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','CompanyId') IS NULL
    ALTER TABLE [SupportTickets] ADD [CompanyId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Subject') IS NULL
    ALTER TABLE [SupportTickets] ADD [Subject] nvarchar(200) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Description') IS NULL
    ALTER TABLE [SupportTickets] ADD [Description] nvarchar(2000) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Category') IS NULL
    ALTER TABLE [SupportTickets] ADD [Category] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Priority') IS NULL
    ALTER TABLE [SupportTickets] ADD [Priority] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','Status') IS NULL
    ALTER TABLE [SupportTickets] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','AssignedTo') IS NULL
    ALTER TABLE [SupportTickets] ADD [AssignedTo] nvarchar(450) NULL;

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','CreatedAt') IS NULL
    ALTER TABLE [SupportTickets] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','UpdatedAt') IS NULL
    ALTER TABLE [SupportTickets] ADD [UpdatedAt] datetime2 NULL;

IF OBJECT_ID(N'[SupportTickets]', N'U') IS NOT NULL AND COL_LENGTH('SupportTickets','ResolvedAt') IS NULL
    ALTER TABLE [SupportTickets] ADD [ResolvedAt] datetime2 NULL;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','Id') IS NULL
    ALTER TABLE [Wallets] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','CompanyId') IS NULL
    ALTER TABLE [Wallets] ADD [CompanyId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','Balance') IS NULL
    ALTER TABLE [Wallets] ADD [Balance] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','TotalCredits') IS NULL
    ALTER TABLE [Wallets] ADD [TotalCredits] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','TotalDebits') IS NULL
    ALTER TABLE [Wallets] ADD [TotalDebits] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','IsSuperAdminWallet') IS NULL
    ALTER TABLE [Wallets] ADD [IsSuperAdminWallet] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','CreatedAt') IS NULL
    ALTER TABLE [Wallets] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Wallets]', N'U') IS NOT NULL AND COL_LENGTH('Wallets','UpdatedAt') IS NULL
    ALTER TABLE [Wallets] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Id') IS NULL
    ALTER TABLE [Loans] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','UserId') IS NULL
    ALTER TABLE [Loans] ADD [UserId] nvarchar(450) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Amount') IS NULL
    ALTER TABLE [Loans] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DurationInMonths') IS NULL
    ALTER TABLE [Loans] ADD [DurationInMonths] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Purpose') IS NULL
    ALTER TABLE [Loans] ADD [Purpose] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Status') IS NULL
    ALTER TABLE [Loans] ADD [Status] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','ApprovedAt') IS NULL
    ALTER TABLE [Loans] ADD [ApprovedAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DueDate') IS NULL
    ALTER TABLE [Loans] ADD [DueDate] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','RejectedAt') IS NULL
    ALTER TABLE [Loans] ADD [RejectedAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','ApprovedBy') IS NULL
    ALTER TABLE [Loans] ADD [ApprovedBy] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','ApprovedByUserId') IS NULL
    ALTER TABLE [Loans] ADD [ApprovedByUserId] nvarchar(450) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','RejectedBy') IS NULL
    ALTER TABLE [Loans] ADD [RejectedBy] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','RejectedByUserId') IS NULL
    ALTER TABLE [Loans] ADD [RejectedByUserId] nvarchar(450) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Reason') IS NULL
    ALTER TABLE [Loans] ADD [Reason] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MandateCreatedAt') IS NULL
    ALTER TABLE [Loans] ADD [MandateCreatedAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','CompanyId') IS NULL
    ALTER TABLE [Loans] ADD [CompanyId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','Message') IS NULL
    ALTER TABLE [Loans] ADD [Message] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','ProductId') IS NULL
    ALTER TABLE [Loans] ADD [ProductId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','IsMandateCreated') IS NULL
    ALTER TABLE [Loans] ADD [IsMandateCreated] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MandateRef') IS NULL
    ALTER TABLE [Loans] ADD [MandateRef] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DisbursementDate') IS NULL
    ALTER TABLE [Loans] ADD [DisbursementDate] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DisbursementReference') IS NULL
    ALTER TABLE [Loans] ADD [DisbursementReference] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MandateStoppedDate') IS NULL
    ALTER TABLE [Loans] ADD [MandateStoppedDate] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MandateStoppedAt') IS NULL
    ALTER TABLE [Loans] ADD [MandateStoppedAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MandateStoppedBy') IS NULL
    ALTER TABLE [Loans] ADD [MandateStoppedBy] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','OfferLetterDocumentId') IS NULL
    ALTER TABLE [Loans] ADD [OfferLetterDocumentId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','OfferLetterUrl') IS NULL
    ALTER TABLE [Loans] ADD [OfferLetterUrl] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','OfferLetterSentAt') IS NULL
    ALTER TABLE [Loans] ADD [OfferLetterSentAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','SignedOfferLetterDocumentId') IS NULL
    ALTER TABLE [Loans] ADD [SignedOfferLetterDocumentId] uniqueidentifier NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','SignedOfferLetterUploadedAt') IS NULL
    ALTER TABLE [Loans] ADD [SignedOfferLetterUploadedAt] datetime2 NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','TotalRepayment') IS NULL
    ALTER TABLE [Loans] ADD [TotalRepayment] decimal(18,2) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','MonthlyRepayment') IS NULL
    ALTER TABLE [Loans] ADD [MonthlyRepayment] decimal(18,2) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DisbursementAmount') IS NULL
    ALTER TABLE [Loans] ADD [DisbursementAmount] decimal(18,2) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','ApplicableFees') IS NULL
    ALTER TABLE [Loans] ADD [ApplicableFees] decimal(18,2) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','AppliedInterest') IS NULL
    ALTER TABLE [Loans] ADD [AppliedInterest] decimal(18,2) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','CreatedAt') IS NULL
    ALTER TABLE [Loans] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','UpdatedAt') IS NULL
    ALTER TABLE [Loans] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','Id') IS NULL
    ALTER TABLE [SupportComments] ADD [Id] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','TicketId') IS NULL
    ALTER TABLE [SupportComments] ADD [TicketId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','UserId') IS NULL
    ALTER TABLE [SupportComments] ADD [UserId] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','Comment') IS NULL
    ALTER TABLE [SupportComments] ADD [Comment] nvarchar(1000) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','IsInternal') IS NULL
    ALTER TABLE [SupportComments] ADD [IsInternal] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[SupportComments]', N'U') IS NOT NULL AND COL_LENGTH('SupportComments','CreatedAt') IS NULL
    ALTER TABLE [SupportComments] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','Id') IS NULL
    ALTER TABLE [WalletTransactions] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','WalletId') IS NULL
    ALTER TABLE [WalletTransactions] ADD [WalletId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','Amount') IS NULL
    ALTER TABLE [WalletTransactions] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','BalanceAfter') IS NULL
    ALTER TABLE [WalletTransactions] ADD [BalanceAfter] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','TransactionType') IS NULL
    ALTER TABLE [WalletTransactions] ADD [TransactionType] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','Description') IS NULL
    ALTER TABLE [WalletTransactions] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','ReferenceId') IS NULL
    ALTER TABLE [WalletTransactions] ADD [ReferenceId] nvarchar(max) NULL;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','InitiatedBy') IS NULL
    ALTER TABLE [WalletTransactions] ADD [InitiatedBy] nvarchar(450) NULL;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','PaystackReference') IS NULL
    ALTER TABLE [WalletTransactions] ADD [PaystackReference] nvarchar(450) NULL;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','CreatedAt') IS NULL
    ALTER TABLE [WalletTransactions] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','Id') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','Email') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [Email] nvarchar(450) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','FirstName') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [FirstName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','LastName') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [LastName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','Employer') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [Employer] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','PhoneNumber') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [PhoneNumber] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','BankCode') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [BankCode] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','AccountNo') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [AccountNo] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','BVN') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [BVN] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','Address') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [Address] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','IdNumber') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [IdNumber] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','DocumentIds') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [DocumentIds] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','CompanyId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [CompanyId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','ProductId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [ProductId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','LoanId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [LoanId] uniqueidentifier NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MaxLoanEligible') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MaxLoanEligible] decimal(18,2) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MinLoanEligible') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MinLoanEligible] decimal(18,2) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MaxTenor') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MaxTenor] int NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MinTenor') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MinTenor] int NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','CurrentStep') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [CurrentStep] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','EmailVerifiedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [EmailVerifiedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','BvnVerifiedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [BvnVerifiedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','DocumentsUploadedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [DocumentsUploadedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','LoanSubmittedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [LoanSubmittedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','LastEmailOtp') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [LastEmailOtp] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','EmailOtpGeneratedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [EmailOtpGeneratedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','LastBvnOtp') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [LastBvnOtp] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','BvnOtpGeneratedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [BvnOtpGeneratedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MonoBvnSessionId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MonoBvnSessionId] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MonoBvnMethod') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MonoBvnMethod] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MonoBvnMethodHint') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MonoBvnMethodHint] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','IsBvnVerified') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [IsBvnVerified] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MonoBvnVerifiedData') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MonoBvnVerifiedData] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','IsOfferLetterAccepted') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [IsOfferLetterAccepted] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','OfferLetterAcceptedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [OfferLetterAcceptedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','DirectDebitMandateId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [DirectDebitMandateId] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','RemitaTransRef') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [RemitaTransRef] nvarchar(max) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MandateGeneratedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MandateGeneratedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MandateActivatedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MandateActivatedAt] datetime2 NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','IsCompleted') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [IsCompleted] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','IsActive') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [IsActive] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','CreatedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','UpdatedAt') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Id') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','CompanyId') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [CompanyId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','LoanId') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [LoanId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','MandateId') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [MandateId] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Reference') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Reference] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','NibssCode') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [NibssCode] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Status') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Status] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','MandateType') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [MandateType] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','DebitType') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [DebitType] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','ReadyToDebit') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [ReadyToDebit] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Approved') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Approved] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','AccountName') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [AccountName] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','AccountNumber') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [AccountNumber] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Bank') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Bank] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','BankCode') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [BankCode] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Customer') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Customer] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','FeeBearer') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [FeeBearer] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Description') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','LiveMode') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [LiveMode] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','StartDate') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [StartDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','EndDate') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [EndDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','InitialDebitDate') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [InitialDebitDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','Amount') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [Amount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','InitialDebitAmount') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [InitialDebitAmount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','TransferDestinationsJson') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [TransferDestinationsJson] nvarchar(max) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','CreatedAt') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','UpdatedAt') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','Id') IS NULL
    ALTER TABLE [Repayments] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','LoanId') IS NULL
    ALTER TABLE [Repayments] ADD [LoanId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','TotalDue') IS NULL
    ALTER TABLE [Repayments] ADD [TotalDue] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','TotalRepaid') IS NULL
    ALTER TABLE [Repayments] ADD [TotalRepaid] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','LastPaymentAt') IS NULL
    ALTER TABLE [Repayments] ADD [LastPaymentAt] datetime2 NULL;

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','AmountUnpaid') IS NULL
    ALTER TABLE [Repayments] ADD [AmountUnpaid] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','Status') IS NULL
    ALTER TABLE [Repayments] ADD [Status] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','CreatedAt') IS NULL
    ALTER TABLE [Repayments] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[Repayments]', N'U') IS NOT NULL AND COL_LENGTH('Repayments','UpdatedAt') IS NULL
    ALTER TABLE [Repayments] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','Id') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','BorrowerApplicationId') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [BorrowerApplicationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','BvnHash') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [BvnHash] nvarchar(64) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','Provider') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [Provider] nvarchar(10) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','CreditScore') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [CreditScore] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','MaxLoanAmount') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [MaxLoanAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','RiskLevel') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [RiskLevel] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','ActiveLoansCount') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [ActiveLoansCount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','TotalOutstandingDebt') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [TotalOutstandingDebt] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','OverallPerformanceStatus') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [OverallPerformanceStatus] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','RecommendedAction') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [RecommendedAction] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','ExpiresAt') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [ExpiresAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','CreatedAt') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','Id') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','BorrowerApplicationId') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [BorrowerApplicationId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','CustomerId') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [CustomerId] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','AuthorisationCode') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [AuthorisationCode] nvarchar(20) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','AccountNumber') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [AccountNumber] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','BankCode') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [BankCode] nvarchar(10) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','BVN') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [BVN] nvarchar(20) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','CompanyName') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [CompanyName] nvarchar(200) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','CustomerName') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [CustomerName] nvarchar(200) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','Category') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [Category] nvarchar(100) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','FirstPaymentDate') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [FirstPaymentDate] datetime2 NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','SalaryCount') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [SalaryCount] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','AverageMonthlySalary') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [AverageMonthlySalary] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','LatestSalaryAmount') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [LatestSalaryAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','LatestPaymentDate') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [LatestPaymentDate] datetime2 NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','MinSalaryAmount') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [MinSalaryAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','MaxSalaryAmount') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [MaxSalaryAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','ConsistentMonths') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [ConsistentMonths] int NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','HasOutstandingLoans') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [HasOutstandingLoans] bit NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','TotalOutstandingAmount') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [TotalOutstandingAmount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','RawRemitaResponse') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [RawRemitaResponse] nvarchar(max) NULL;

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','CreatedAt') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaSalaryHistories]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryHistories','UpdatedAt') IS NULL
    ALTER TABLE [RemitaSalaryHistories] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','Id') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','RemitaSalaryHistoryId') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [RemitaSalaryHistoryId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','PaymentDate') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [PaymentDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','Amount') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','AccountNumber') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [AccountNumber] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','BankCode') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [BankCode] nvarchar(10) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[RemitaSalaryPayments]', N'U') IS NOT NULL AND COL_LENGTH('RemitaSalaryPayments','CreatedAt') IS NULL
    ALTER TABLE [RemitaSalaryPayments] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','Id') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [Id] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','TransactionReference') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [TransactionReference] nvarchar(100) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','FromAccount') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [FromAccount] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','ToAccount') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [ToAccount] nvarchar(20) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','Amount') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [Amount] decimal(18,2) NOT NULL DEFAULT 0;

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','Remarks') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [Remarks] nvarchar(500) NULL;

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','Status') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [Status] nvarchar(50) NOT NULL DEFAULT N'';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','RawResponse') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [RawResponse] nvarchar(4000) NULL;

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','CreatedAt') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [CreatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('EWalletTransactions','UpdatedAt') IS NULL
    ALTER TABLE [EWalletTransactions] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','PassportUrl') IS NULL
    ALTER TABLE [EWallets] ADD [PassportUrl] nvarchar(500) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Address') IS NULL
    ALTER TABLE [EWallets] ADD [Address] nvarchar(200) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Alias') IS NULL
    ALTER TABLE [EWallets] ADD [Alias] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Bvn') IS NULL
    ALTER TABLE [EWallets] ADD [Bvn] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','BvnVerified') IS NULL
    ALTER TABLE [EWallets] ADD [BvnVerified] bit NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','City') IS NULL
    ALTER TABLE [EWallets] ADD [City] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','CustomerRawResponse') IS NULL
    ALTER TABLE [EWallets] ADD [CustomerRawResponse] nvarchar(4000) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Dob') IS NULL
    ALTER TABLE [EWallets] ADD [Dob] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','EmailAddress') IS NULL
    ALTER TABLE [EWallets] ADD [EmailAddress] nvarchar(256) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','FirstName') IS NULL
    ALTER TABLE [EWallets] ADD [FirstName] nvarchar(100) NOT NULL DEFAULT '';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Gender') IS NULL
    ALTER TABLE [EWallets] ADD [Gender] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','KycTier') IS NULL
    ALTER TABLE [EWallets] ADD [KycTier] int NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','LastName') IS NULL
    ALTER TABLE [EWallets] ADD [LastName] nvarchar(100) NOT NULL DEFAULT '';

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','MaritalStatus') IS NULL
    ALTER TABLE [EWallets] ADD [MaritalStatus] nvarchar(50) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','MiddleName') IS NULL
    ALTER TABLE [EWallets] ADD [MiddleName] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','MothersMaidenName') IS NULL
    ALTER TABLE [EWallets] ADD [MothersMaidenName] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinAddress') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinAddress] nvarchar(200) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinFirstName') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinFirstName] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinMobileNumber') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinMobileNumber] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinOtherNames') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinOtherNames] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinRelationship') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinRelationship] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NextOfKinSurname') IS NULL
    ALTER TABLE [EWallets] ADD [NextOfKinSurname] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Nin') IS NULL
    ALTER TABLE [EWallets] ADD [Nin] nvarchar(20) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','NinVerified') IS NULL
    ALTER TABLE [EWallets] ADD [NinVerified] bit NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','Occupation') IS NULL
    ALTER TABLE [EWallets] ADD [Occupation] nvarchar(100) NULL;

IF OBJECT_ID(N'[EWallets]', N'U') IS NOT NULL AND COL_LENGTH('EWallets','WalletRawResponse') IS NULL
    ALTER TABLE [EWallets] ADD [WalletRawResponse] nvarchar(1000) NULL;

IF OBJECT_ID(N'[BorrowerApplications]', N'U') IS NOT NULL AND COL_LENGTH('BorrowerApplications','MonoCustomerId') IS NULL
    ALTER TABLE [BorrowerApplications] ADD [MonoCustomerId] nvarchar(max) NULL;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','PositiveFactorsJson') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [PositiveFactorsJson] nvarchar(max) NULL;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','RiskFactorsJson') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [RiskFactorsJson] nvarchar(max) NULL;
COMMIT;
GO
