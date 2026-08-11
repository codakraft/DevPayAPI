-- Clears the pending EF migration chain on prod and adds the disbursement
-- idempotency columns to Loans.
--
-- Three migrations are pending and the first one FAILS on its own, so nothing
-- behind it can apply:
--     20260705093918_AddCreditAnalysisFactorsJson    <- blocker
--     20260706145441_AddMonoUrlToMandateReference
--     20260811181021_AddDisbursementIdempotencyToLoan
--
-- The blocker fails with SQL error 2705 ("Column name 'PositiveFactorsJson' ...
-- is specified more than once") because prod-column-reconcile.sql already added
-- those columns (see its lines 1501-1508) without writing any __EFMigrationsHistory
-- rows. The schema is ahead of the history, so EF replays work that is already done.
--
-- This script reconciles both halves: it adds any column still missing, then
-- records all three migrations as applied so `dotnet ef database update` becomes
-- a no-op and the app's startup Migrate() call stops failing.
--
-- Every ADD is guarded by IF COL_LENGTH(...) IS NULL, matching the convention in
-- prod-column-reconcile.sql. Idempotent and safe to re-run. Wrapped in a
-- transaction. All changes are additive; no existing data is modified.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- ── 20260705093918_AddCreditAnalysisFactorsJson ──────────────────────────────

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','PositiveFactorsJson') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [PositiveFactorsJson] nvarchar(max) NULL;

IF OBJECT_ID(N'[MonoCreditAnalysisRecords]', N'U') IS NOT NULL AND COL_LENGTH('MonoCreditAnalysisRecords','RiskFactorsJson') IS NULL
    ALTER TABLE [MonoCreditAnalysisRecords] ADD [RiskFactorsJson] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260705093918_AddCreditAnalysisFactorsJson')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260705093918_AddCreditAnalysisFactorsJson', N'9.0.6');

-- ── 20260706145441_AddMonoUrlToMandateReference ──────────────────────────────

IF OBJECT_ID(N'[MonoMandateReferences]', N'U') IS NOT NULL AND COL_LENGTH('MonoMandateReferences','MonoUrl') IS NULL
    ALTER TABLE [MonoMandateReferences] ADD [MonoUrl] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260706145441_AddMonoUrlToMandateReference')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260706145441_AddMonoUrlToMandateReference', N'9.0.6');

-- ── 20260811181021_AddDisbursementIdempotencyToLoan ──────────────────────────
-- DisbursementTransactionRef holds the reference sent to Providus. It is persisted
-- before the transfer and reused on every retry, so Providus's duplicate guard
-- (response 7709) can recognise a repeat attempt instead of paying out twice.
--
-- DisbursementOutcomeUnknown latches a loan whose transfer outcome could not be
-- established (timeout, dropped connection). While it is set, DisburseLoanAsync
-- refuses to disburse again until the reference has been reconciled with Providus
-- via GetNIPTransactionStatus.

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DisbursementTransactionRef') IS NULL
    ALTER TABLE [Loans] ADD [DisbursementTransactionRef] nvarchar(max) NULL;

IF OBJECT_ID(N'[Loans]', N'U') IS NOT NULL AND COL_LENGTH('Loans','DisbursementOutcomeUnknown') IS NULL
    ALTER TABLE [Loans] ADD [DisbursementOutcomeUnknown] bit NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260811181021_AddDisbursementIdempotencyToLoan')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260811181021_AddDisbursementIdempotencyToLoan', N'9.0.6');

COMMIT;

-- ── Verification ─────────────────────────────────────────────────────────────
-- Expect 5 rows from the first query and the 3 migration ids from the second.

SELECT OBJECT_NAME(c.object_id) AS [Table], c.name AS [Column]
FROM sys.columns c
WHERE (c.object_id = OBJECT_ID(N'[MonoCreditAnalysisRecords]') AND c.name IN (N'PositiveFactorsJson', N'RiskFactorsJson'))
   OR (c.object_id = OBJECT_ID(N'[MonoMandateReferences]')      AND c.name = N'MonoUrl')
   OR (c.object_id = OBJECT_ID(N'[Loans]')                      AND c.name IN (N'DisbursementTransactionRef', N'DisbursementOutcomeUnknown'));

SELECT [MigrationId], [ProductVersion]
FROM [__EFMigrationsHistory]
WHERE [MigrationId] IN (
    N'20260705093918_AddCreditAnalysisFactorsJson',
    N'20260706145441_AddMonoUrlToMandateReference',
    N'20260811181021_AddDisbursementIdempotencyToLoan');
