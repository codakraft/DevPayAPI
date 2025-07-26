# Fixed Foreign Key Constraint Issue for System-Initiated Wallet Transactions

## Problem Description
The application was throwing a foreign key constraint error when trying to create wallet transactions for system-initiated operations (like OTP fee deductions):

```
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_WalletTransactions_AspNetUsers_InitiatedBy". The conflict occurred in database "LendingDev", table "dbo.AspNetUsers", column 'Id'.
```

This occurred because the system was trying to pass "SYSTEM" as the `InitiatedBy` value, but this field has a foreign key constraint to the `AspNetUsers` table, and "SYSTEM" is not a valid user ID.

## Root Cause Analysis
1. `WalletTransaction.InitiatedBy` is a `string?` field with a foreign key relationship to `AspNetUsers.Id`
2. The OTP fee deduction process was passing "SYSTEM" as the user ID for system-initiated transactions
3. The database constraint requires that any non-null value in `InitiatedBy` must exist in the `AspNetUsers` table
4. "SYSTEM" does not exist as a user ID in the `AspNetUsers` table

## Solution Implemented

### 1. Updated Interface Signatures
Modified the wallet service interfaces to accept nullable `userId` parameters for system operations:

**File:** `IWalletService.cs`
- `Task<bool> TransferFundsAsync(..., string? userId)` (was `string userId`)
- `Task<bool> DebitWalletAsync(DebitWalletDto debitWalletDto, string? userId)` (was `string userId`)
- `Task<bool> CreditWalletAsync(..., string? userId)` (was `string userId`)

### 2. Updated Implementation Signatures
Updated the corresponding implementations in `WalletService.cs` to match the interface changes.

### 3. Modified System Transaction Calls
Changed the `DeductOtpFeeAsync` method in `BorrowerOnboardingService.cs` to pass `null` instead of "SYSTEM":

```csharp
// Before: "SYSTEM" // System-initiated transfer
// After: null // System-initiated transfer - null for system transactions
```

### 4. Database Compatibility
The `WalletTransaction.InitiatedBy` field is already nullable (`string?`), so setting it to `null` for system transactions is perfectly valid and doesn't violate any database constraints.

## Impact Analysis

### ✅ **Fixed Issues:**
- System-initiated OTP fee deductions now work without foreign key constraint errors
- Wallet transactions can be created for automated system processes
- No more "SYSTEM" user ID conflicts

### ✅ **Maintained Functionality:**
- User-initiated transactions still work normally (WalletController passes valid user IDs)
- Existing wallet operations remain unaffected
- All foreign key relationships are preserved

### ✅ **Database Integrity:**
- No changes to database schema required
- Foreign key constraints remain intact
- Transaction audit trail is maintained (null indicates system-initiated)

## Testing Results
- ✅ Project builds successfully
- ✅ All wallet service method signatures are consistent
- ✅ No breaking changes to existing functionality
- ✅ System-initiated transactions can now be created without errors

## Files Modified

### Interface Changes:
- `LendingSolution.Application\Services\Interfaces\IWalletService.cs`

### Implementation Changes:
- `LendingSolution.Application\Services\Implementations\WalletService.cs`
- `LendingSolution.Application\Services\Implementations\BorrowerOnboardingService.cs`

## Technical Details

### Transaction Attribution:
- **User-initiated transactions:** `InitiatedBy` contains the actual user ID
- **System-initiated transactions:** `InitiatedBy` is `null` (indicating system operation)

### OTP Fee Flow:
1. User triggers OTP generation during borrower onboarding
2. `DeductOtpFeeAsync` is called with company ID
3. Company wallet is debited, SuperAdmin wallet is credited
4. Both transactions are recorded with `InitiatedBy = null` (system operation)
5. No foreign key constraint violations occur

This fix ensures that all system-automated financial operations (like OTP fees) can be processed without database constraint violations while maintaining proper audit trails and user attribution for manual operations.
