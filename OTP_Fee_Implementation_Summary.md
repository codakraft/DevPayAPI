# OTP Fee Deduction Implementation Summary

## Overview
Implemented automatic fee deduction from company wallets to SuperAdmin wallet every time an OTP (One-Time Password) is generated in the system.

## Changes Made

### 1. Added New Wallet Transaction Type
**File**: `LendingSolution.Core\Models\Wallet.cs`
- Added `OtpFee = 10` to the `WalletTransactionType` enum
- This allows proper categorization of OTP-related wallet transactions

### 2. Enhanced BorrowerOnboardingService
**File**: `LendingSolution.Application\Services\Implementations\BorrowerOnboardingService.cs`

#### Added Dependencies:
- `IWalletService` - For wallet operations
- `ISettingsService` - For retrieving OTP fee configuration

#### New Private Method:
```csharp
private async Task<bool> DeductOtpFeeAsync(Guid companyId, string otpType = "OTP")
```

**Functionality**:
- Retrieves OTP fee from system settings
- Validates company wallet exists and has sufficient balance
- Gets or creates SuperAdmin wallet if needed
- Transfers OTP fee from company wallet to SuperAdmin wallet
- Records transaction with proper description and reference
- Handles errors gracefully with appropriate user messages

#### Modified OTP Generation Methods:
1. **`GenerateEmailOtpAsync`**:
   - Now deducts OTP fee before generating email OTP
   - Fee deduction happens before OTP generation to prevent partial operations

2. **`GenerateBvnOtpAsync`**:
   - Now deducts OTP fee before generating BVN verification OTP
   - Maintains same flow but adds fee processing

3. **`ResendStep1EmailOtpAsync`**:
   - Now deducts OTP fee when resending email OTPs
   - Prevents abuse by charging for each resend attempt

## Fee Configuration
- OTP fee amount is configured in the `Settings` table
- Default OTP fee is 20 (configurable via Settings API)
- Fee type can be FIXED or PERCENTAGE (currently using FIXED)
- If OTP fee is 0 or negative, no deduction occurs

## Transaction Flow
1. **OTP Request Initiated** → Company requests OTP generation
2. **Fee Validation** → System checks if company wallet has sufficient balance
3. **Balance Check** → If insufficient funds, operation fails with clear error message
4. **Fee Deduction** → Amount transferred from company wallet to SuperAdmin wallet
5. **Transaction Recording** → Both debit and credit transactions recorded with proper references
6. **OTP Generation** → OTP is generated and sent only after successful fee processing

## Error Handling
- **Insufficient Balance**: Clear error message directing user to fund wallet
- **Missing Wallet**: Error message directing user to contact support
- **System Errors**: Generic error message with proper exception handling
- **Partial Failures**: Fee deduction happens before OTP generation to prevent inconsistencies

## Wallet Transaction Details
Each OTP generation creates the following transactions:

### Company Wallet (Debit):
- **Amount**: Negative value (deduction)
- **Type**: `WalletTransactionType.OtpFee`
- **Description**: "{OTP_TYPE} generation fee - Company: {CompanyName}"
- **Initiated By**: "SYSTEM"

### SuperAdmin Wallet (Credit):
- **Amount**: Positive value (income)
- **Type**: `WalletTransactionType.OtpFee`
- **Description**: "{OTP_TYPE} generation fee - Company: {CompanyName}"
- **Initiated By**: "SYSTEM"

## Affected OTP Operations
1. **Email OTP Generation** - Step 1 of borrower onboarding
2. **Email OTP Resend** - When users request OTP resend
3. **BVN OTP Generation** - Step 2 of borrower onboarding
4. **All future OTP generations** - Framework is extensible for new OTP types

## Benefits
1. **Revenue Generation**: Automatic fee collection for OTP services
2. **Usage Tracking**: All OTP generations are tracked in wallet transactions
3. **Balance Management**: Prevents OTP spam by requiring sufficient wallet balance
4. **Audit Trail**: Complete transaction history for all OTP-related fees
5. **Configurable Fees**: OTP fees can be adjusted via Settings API
6. **Scalable Design**: Easy to extend for new OTP types and fee structures

## Technical Notes
- Fee deduction is atomic - either fully succeeds or fully fails
- Uses existing wallet transfer mechanisms for reliability
- Integrates with existing Settings system for configuration
- Maintains backward compatibility
- No changes required to existing API endpoints
- Zero downtime deployment possible

## Testing Considerations
- Test with sufficient wallet balance
- Test with insufficient wallet balance
- Test with missing company wallet
- Test with missing SuperAdmin wallet (auto-creation)
- Test with zero/negative OTP fee (should skip deduction)
- Verify transaction records are created correctly
- Verify error messages are user-friendly
