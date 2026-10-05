-- Audit log: who-did-it and categories (QA round 2, part 3).
-- 1. Adds AuditLogs.UserName.
-- 2. Makes UserId / UserEmail / UserName mean the person who acted on existing rows.
-- 3. Moves existing rows to the new categories (Authentication; role and access changes -> Security).
-- Idempotent and safe to re-run. Run with sqlcmd or SSMS (uses GO batch separators).

IF OBJECT_ID(N'[AuditLogs]', N'U') IS NOT NULL AND COL_LENGTH('AuditLogs','UserName') IS NULL
    ALTER TABLE [AuditLogs] ADD [UserName] nvarchar(max) NULL;
GO

BEGIN TRANSACTION;

-- Role/status/edit rows stored the acting admin in UserId but the affected user's email in UserEmail.
-- The affected user is already in EntityId; replace the email with the actor's.
UPDATE a
SET a.UserEmail = u.Email
FROM [AuditLogs] a
JOIN [AspNetUsers] u ON u.Id = a.UserId
WHERE a.Action IN (N'RoleAssigned', N'RoleRemoved', N'UserActivated', N'UserDeactivated', N'UserUpdated')
  AND (a.UserEmail IS NULL OR a.UserEmail <> u.Email);

-- Old admin-creation rows stored the new account as the actor; the real creator was never recorded.
-- Clear the actor rather than show the new admin as having created themselves (EntityId keeps the new account).
UPDATE [AuditLogs]
SET UserId = NULL, UserEmail = NULL, UserName = NULL
WHERE Action IN (N'SuperAdminCreated', N'AdminCreated')
  AND UserId IS NOT NULL
  AND UserId = EntityId;

-- Fill in the actor's email, name and company from UserId.
UPDATE a
SET a.UserEmail = COALESCE(a.UserEmail, u.Email),
    a.UserName  = COALESCE(a.UserName, NULLIF(LTRIM(RTRIM(CONCAT(u.FirstName, N' ', u.LastName))), N'')),
    a.CompanyId = COALESCE(a.CompanyId, TRY_CONVERT(uniqueidentifier, u.CompanyId))
FROM [AuditLogs] a
JOIN [AspNetUsers] u ON u.Id = a.UserId
WHERE a.UserEmail IS NULL OR a.UserName IS NULL OR a.CompanyId IS NULL;

-- Rows logged by email only (failed logins, login OTP failures): name and company from the matching account.
UPDATE a
SET a.UserName  = COALESCE(a.UserName, NULLIF(LTRIM(RTRIM(CONCAT(u.FirstName, N' ', u.LastName))), N'')),
    a.CompanyId = COALESCE(a.CompanyId, TRY_CONVERT(uniqueidentifier, u.CompanyId))
FROM [AuditLogs] a
JOIN [AspNetUsers] u ON u.NormalizedEmail = UPPER(a.UserEmail)
WHERE a.UserId IS NULL
  AND a.Category IN (N'Security', N'Authentication')
  AND (a.UserName IS NULL OR a.CompanyId IS NULL);

-- Categories
UPDATE [AuditLogs]
SET Category = N'Authentication'
WHERE Action IN (N'LoginSuccessful', N'AdminLoginSuccessful', N'AdminLoginMfaInitiated')
  AND Category <> N'Authentication';

UPDATE [AuditLogs]
SET Category = N'Security'
WHERE Action IN (N'RoleAssigned', N'RoleRemoved', N'UserActivated', N'UserDeactivated')
  AND Category <> N'Security';

COMMIT TRANSACTION;
GO
