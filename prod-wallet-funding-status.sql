-- Wallet funding status (QA round 2, part 4).
-- Adds WalletTransactions.Status and CompletedAt. No data is changed: existing rows keep
-- Status NULL, which the API treats as "can't be completed", so old Paystack references
-- can't be replayed to credit a wallet again.
-- Idempotent and safe to re-run. Run before deploying the API change.

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','Status') IS NULL
    ALTER TABLE [WalletTransactions] ADD [Status] int NULL;

IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NOT NULL AND COL_LENGTH('WalletTransactions','CompletedAt') IS NULL
    ALTER TABLE [WalletTransactions] ADD [CompletedAt] datetime2 NULL;
