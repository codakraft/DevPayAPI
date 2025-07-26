# Auto-Create Company Wallet Enhancement

## Overview
Enhanced the `BorrowerOnboardingService` to automatically create a company wallet when one doesn't exist during OTP fee deduction process.

## Problem Solved
Previously, if a company didn't have a wallet when trying to generate an OTP, the system would throw an error: "Company wallet not found. Please contact support." This created a poor user experience and required manual intervention.

## Solution Implemented
Modified the `DeductOtpFeeAsync` method in `BorrowerOnboardingService.cs` to automatically create a company wallet if one doesn't exist.

### Code Changes

**File:** `LendingSolution.Application\Services\Implementations\BorrowerOnboardingService.cs`

**Before:**
```csharp
// Get company wallet
var companyWallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
if (companyWallet == null)
{
    throw new AppException("Company wallet not found. Please contact support.", 404);
}
```

**After:**
```csharp
// Get or create company wallet
var companyWallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
if (companyWallet == null)
{
    // Automatically create a wallet for the company
    companyWallet = await _walletService.CreateCompanyWalletAsync(companyId);
}
```

## Benefits

1. **Improved User Experience**: No more errors when companies don't have wallets
2. **Seamless OTP Generation**: OTP generation process continues smoothly even for new companies
3. **Automatic Wallet Provisioning**: New company wallets are created automatically when needed
4. **Consistent with Existing Pattern**: The WalletController already implements similar auto-creation logic

## Technical Details

- The enhancement uses the existing `IWalletService.CreateCompanyWalletAsync()` method
- New wallets are created with:
  - Balance: 0
  - TotalCredits: 0
  - TotalDebits: 0
  - IsSuperAdminWallet: false
- The method follows the existing pattern used in the WalletController

## Testing
- Project builds successfully without errors
- No breaking changes to existing functionality
- Enhancement is backward compatible

## Impact Areas
- **OTP Generation**: All OTP generation methods now benefit from auto-wallet creation
- **Company Onboarding**: New companies can generate OTPs immediately without manual wallet setup
- **System Administration**: Reduced need for manual wallet creation interventions

## Related Files
- `BorrowerOnboardingService.cs` - Main implementation
- `IWalletService.cs` - Interface used
- `WalletService.cs` - Wallet creation logic
- `planner` - Updated with completion status

This enhancement ensures a smoother user experience and reduces administrative overhead while maintaining system integrity.
