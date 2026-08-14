/*
    Creates the AppLogs table in prod.

    AppLogs is defined by migration 20260529112519_AddEWalletsTable, which the
    __EFMigrationsHistory table already reports as applied — so the history says
    this table exists while the database does not have it. That gap is why no
    migration change is needed here: EF already believes the work is done, and
    running `dotnet ef database update` will not create it. Only this CREATE will.

    Until this runs, DatabaseLoggerProvider (registered in Program.cs:14 at
    LogLevel.Information) fails on every insert. The failure is invisible because
    DatabaseLoggerProvider.cs wraps each row in its own swallowing try/catch, so
    logging has been silently discarded rather than erroring.

    Column definitions below are copied exactly from the migration, so the table
    matches what EF expects. Idempotent: safe to run more than once.
*/

IF OBJECT_ID(N'[dbo].[AppLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AppLogs] (
        [Id]         bigint         IDENTITY(1,1) NOT NULL,
        [Timestamp]  datetime2      NOT NULL,
        [Level]      nvarchar(20)   NOT NULL,
        [Category]   nvarchar(512)  NULL,
        [Message]    nvarchar(max)  NOT NULL,
        [Exception]  nvarchar(max)  NULL,
        [EventId]    nvarchar(100)  NULL,
        CONSTRAINT [PK_AppLogs] PRIMARY KEY ([Id])
    );

    PRINT 'Created table dbo.AppLogs';
END
ELSE
    PRINT 'Skipped: dbo.AppLogs already exists';
GO

/*
    OPTIONAL — an index on Timestamp.

    Not part of the EF model, so leaving it out keeps the table exactly as EF
    defines it. Worth adding if you intend to query these logs: the table receives
    every Information-level message the app emits, so it grows quickly and any
    "what happened around time T" query will otherwise scan the whole table.

    Uncomment to apply.
*/

-- IF OBJECT_ID(N'[dbo].[AppLogs]', N'U') IS NOT NULL
--    AND NOT EXISTS (SELECT 1 FROM sys.indexes
--                    WHERE [name] = N'IX_AppLogs_Timestamp'
--                      AND [object_id] = OBJECT_ID(N'[dbo].[AppLogs]'))
-- BEGIN
--     CREATE NONCLUSTERED INDEX [IX_AppLogs_Timestamp]
--         ON [dbo].[AppLogs] ([Timestamp] DESC);
--     PRINT 'Created index IX_AppLogs_Timestamp';
-- END
-- GO

-- ── Verify ───────────────────────────────────────────────────────────────────
-- Expect 7 column rows. The table starts empty; new rows appear within ~5 seconds
-- of the next log write, since DatabaseLoggerProvider flushes on a 5s timer.

SELECT  c.[name] AS [Column], TYPE_NAME(c.[user_type_id]) AS [Type],
        c.[max_length], c.[is_nullable], c.[is_identity]
FROM    sys.columns c
WHERE   c.[object_id] = OBJECT_ID(N'[dbo].[AppLogs]')
ORDER BY c.[column_id];
