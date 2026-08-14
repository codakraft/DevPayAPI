/*
    Reports which tables the EF model expects but prod does not have.

    Why this is needed: prod-column-reconcile.sql guards every statement with
        IF OBJECT_ID(N'[Table]', N'U') IS NOT NULL AND COL_LENGTH(...) IS NULL
    so it adds columns to tables that already exist and SILENTLY SKIPS tables that
    are missing entirely. Combined with migration history rows that were inserted
    by hand, a table can be recorded as created while never having been created —
    which is what happened to AppLogs (created by migration
    20260529112519_AddEWalletsTable, which the history reports as applied).

    Read-only. Safe to run any time.

    The 35 names below come from ToTable(...) in ApplicationDbContextModelSnapshot.cs
    and are the complete set the model expects, including the ASP.NET Identity tables.
*/

WITH [Expected]([TableName]) AS (
    SELECT * FROM (VALUES
        (N'AdminSettings'),
        (N'AppLogs'),
        (N'Approvals'),
        (N'AspNetRoleClaims'),
        (N'AspNetRoles'),
        (N'AspNetUserClaims'),
        (N'AspNetUserLogins'),
        (N'AspNetUserRoles'),
        (N'AspNetUserTokens'),
        (N'AspNetUsers'),
        (N'AuditLogs'),
        (N'BorrowerApplications'),
        (N'Companies'),
        (N'Disbursements'),
        (N'Documents'),
        (N'EWalletTransactions'),
        (N'EWallets'),
        (N'Employees'),
        (N'LoanProducts'),
        (N'Loans'),
        (N'MfaSessions'),
        (N'MonoBvnVerificationRecords'),
        (N'MonoCreditAnalysisRecords'),
        (N'MonoMandateReferences'),
        (N'Otps'),
        (N'RefreshTokens'),
        (N'RemitaLoanCollectionNotifications'),
        (N'RemitaSalaryHistories'),
        (N'RemitaSalaryPayments'),
        (N'Repayments'),
        (N'Settings'),
        (N'SupportComments'),
        (N'SupportTickets'),
        (N'WalletTransactions'),
        (N'Wallets')
    ) AS t([TableName])
)
SELECT      e.[TableName],
            CASE WHEN t.[object_id] IS NULL THEN 'MISSING' ELSE 'present' END AS [State]
FROM        [Expected] e
LEFT JOIN   sys.tables t
       ON   t.[name] = e.[TableName]
      AND   t.[schema_id] = SCHEMA_ID(N'dbo')
ORDER BY    CASE WHEN t.[object_id] IS NULL THEN 0 ELSE 1 END,  -- missing first
            e.[TableName];


-- Any table in prod that the model does NOT expect (leftovers, renames, typos).
-- __EFMigrationsHistory is excluded; it is EF's own bookkeeping table.

SELECT      t.[name] AS [UnexpectedTable]
FROM        sys.tables t
WHERE       t.[schema_id] = SCHEMA_ID(N'dbo')
  AND       t.[name] <> N'__EFMigrationsHistory'
  AND       t.[name] NOT IN (
                N'AdminSettings', N'AppLogs', N'Approvals', N'AspNetRoleClaims',
                N'AspNetRoles', N'AspNetUserClaims', N'AspNetUserLogins',
                N'AspNetUserRoles', N'AspNetUserTokens', N'AspNetUsers',
                N'AuditLogs', N'BorrowerApplications', N'Companies',
                N'Disbursements', N'Documents', N'EWalletTransactions',
                N'EWallets', N'Employees', N'LoanProducts', N'Loans',
                N'MfaSessions', N'MonoBvnVerificationRecords',
                N'MonoCreditAnalysisRecords', N'MonoMandateReferences', N'Otps',
                N'RefreshTokens', N'RemitaLoanCollectionNotifications',
                N'RemitaSalaryHistories', N'RemitaSalaryPayments', N'Repayments',
                N'Settings', N'SupportComments', N'SupportTickets',
                N'WalletTransactions', N'Wallets')
ORDER BY    t.[name];
