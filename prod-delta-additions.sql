-- Delta: EWallet + Mono feature additions only.
-- Object-guarded (IF NOT EXISTS ...) so any object already present in prod is skipped.
-- Safe to re-run. Wrapped in a transaction for atomicity.

BEGIN TRANSACTION;

IF OBJECT_ID(N'[EWallets]', N'U') IS NULL
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

IF OBJECT_ID(N'[MonoBvnVerificationRecords]', N'U') IS NULL
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

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NULL
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

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NULL
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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoBvnVerificationRecords_BvnHash' AND object_id = OBJECT_ID(N'[MonoBvnVerificationRecords]'))
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_BvnHash] ON [MonoBvnVerificationRecords] ([BvnHash]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoBvnVerificationRecords_CreatedAt' AND object_id = OBJECT_ID(N'[MonoBvnVerificationRecords]'))
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_CreatedAt] ON [MonoBvnVerificationRecords] ([CreatedAt]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoBvnVerificationRecords_ExpiresAt' AND object_id = OBJECT_ID(N'[MonoBvnVerificationRecords]'))
BEGIN
    CREATE INDEX [IX_MonoBvnVerificationRecords_ExpiresAt] ON [MonoBvnVerificationRecords] ([ExpiresAt]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoCreditAnalysisRecords_BorrowerApplicationId' AND object_id = OBJECT_ID(N'[MonoCreditAnalysisRecords]'))
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_BorrowerApplicationId] ON [MonoCreditAnalysisRecords] ([BorrowerApplicationId]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoCreditAnalysisRecords_BvnHash' AND object_id = OBJECT_ID(N'[MonoCreditAnalysisRecords]'))
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_BvnHash] ON [MonoCreditAnalysisRecords] ([BvnHash]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoCreditAnalysisRecords_CreatedAt' AND object_id = OBJECT_ID(N'[MonoCreditAnalysisRecords]'))
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_CreatedAt] ON [MonoCreditAnalysisRecords] ([CreatedAt]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoCreditAnalysisRecords_ExpiresAt' AND object_id = OBJECT_ID(N'[MonoCreditAnalysisRecords]'))
BEGIN
    CREATE INDEX [IX_MonoCreditAnalysisRecords_ExpiresAt] ON [MonoCreditAnalysisRecords] ([ExpiresAt]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoMandateReferences_CompanyId' AND object_id = OBJECT_ID(N'[MonoMandateReferences]'))
BEGIN
    CREATE INDEX [IX_MonoMandateReferences_CompanyId] ON [MonoMandateReferences] ([CompanyId]);
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_MonoMandateReferences_LoanId' AND object_id = OBJECT_ID(N'[MonoMandateReferences]'))
BEGIN
    CREATE INDEX [IX_MonoMandateReferences_LoanId] ON [MonoMandateReferences] ([LoanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529112519_AddEWalletsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529112519_AddEWalletsTable', N'9.0.6');
END;

IF COL_LENGTH('EWallets','PassportUrl') IS NULL ALTER TABLE [EWallets] ADD [PassportUrl] nvarchar(500) NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EWallets') AND name = 'IsDefault' AND is_nullable = 0) ALTER TABLE [EWallets] ALTER COLUMN [IsDefault] bit NULL;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('EWallets') AND name = 'EmbedlyWalletId' AND is_nullable = 0) ALTER TABLE [EWallets] ALTER COLUMN [EmbedlyWalletId] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','Address') IS NULL ALTER TABLE [EWallets] ADD [Address] nvarchar(200) NULL;

IF COL_LENGTH('EWallets','Alias') IS NULL ALTER TABLE [EWallets] ADD [Alias] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','Bvn') IS NULL ALTER TABLE [EWallets] ADD [Bvn] nvarchar(20) NULL;

IF COL_LENGTH('EWallets','BvnVerified') IS NULL ALTER TABLE [EWallets] ADD [BvnVerified] bit NULL;

IF COL_LENGTH('EWallets','City') IS NULL ALTER TABLE [EWallets] ADD [City] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','CustomerRawResponse') IS NULL ALTER TABLE [EWallets] ADD [CustomerRawResponse] nvarchar(4000) NULL;

IF COL_LENGTH('EWallets','Dob') IS NULL ALTER TABLE [EWallets] ADD [Dob] nvarchar(20) NULL;

IF COL_LENGTH('EWallets','EmailAddress') IS NULL ALTER TABLE [EWallets] ADD [EmailAddress] nvarchar(256) NULL;

IF COL_LENGTH('EWallets','FirstName') IS NULL ALTER TABLE [EWallets] ADD [FirstName] nvarchar(100) NOT NULL DEFAULT '';

IF COL_LENGTH('EWallets','Gender') IS NULL ALTER TABLE [EWallets] ADD [Gender] nvarchar(20) NULL;

IF COL_LENGTH('EWallets','KycTier') IS NULL ALTER TABLE [EWallets] ADD [KycTier] int NULL;

IF COL_LENGTH('EWallets','LastName') IS NULL ALTER TABLE [EWallets] ADD [LastName] nvarchar(100) NOT NULL DEFAULT '';

IF COL_LENGTH('EWallets','MaritalStatus') IS NULL ALTER TABLE [EWallets] ADD [MaritalStatus] nvarchar(50) NULL;

IF COL_LENGTH('EWallets','MiddleName') IS NULL ALTER TABLE [EWallets] ADD [MiddleName] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','MothersMaidenName') IS NULL ALTER TABLE [EWallets] ADD [MothersMaidenName] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','NextOfKinAddress') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinAddress] nvarchar(200) NULL;

IF COL_LENGTH('EWallets','NextOfKinFirstName') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinFirstName] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','NextOfKinMobileNumber') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinMobileNumber] nvarchar(20) NULL;

IF COL_LENGTH('EWallets','NextOfKinOtherNames') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinOtherNames] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','NextOfKinRelationship') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinRelationship] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','NextOfKinSurname') IS NULL ALTER TABLE [EWallets] ADD [NextOfKinSurname] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','Nin') IS NULL ALTER TABLE [EWallets] ADD [Nin] nvarchar(20) NULL;

IF COL_LENGTH('EWallets','NinVerified') IS NULL ALTER TABLE [EWallets] ADD [NinVerified] bit NULL;

IF COL_LENGTH('EWallets','Occupation') IS NULL ALTER TABLE [EWallets] ADD [Occupation] nvarchar(100) NULL;

IF COL_LENGTH('EWallets','WalletRawResponse') IS NULL ALTER TABLE [EWallets] ADD [WalletRawResponse] nvarchar(1000) NULL;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260530074251_AddEWalletKycColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260530074251_AddEWalletKycColumns', N'9.0.6');
END;

IF OBJECT_ID(N'[EWalletTransactions]', N'U') IS NULL
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

IF COL_LENGTH('BorrowerApplications','MonoCustomerId') IS NULL
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
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260618094850_CreateMonoMandateReferencesTableFix', N'9.0.6');
END;


COMMIT;
GO
