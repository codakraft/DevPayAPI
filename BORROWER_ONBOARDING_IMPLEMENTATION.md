# Borrower Onboarding Implementation

## Overview
This document describes the implementation of the new step-by-step borrower onboarding flow for the Lending Solution system.

## Implementation Status: ✅ COMPLETED

### What Was Implemented

#### 1. **New Step-by-Step Onboarding Flow**
The borrower onboarding process has been completely refactored into a 4-step process with OTP validation:

##### Step 1: Basic Information Collection
- **Endpoint**: `POST /api/borrower/step1`
- **Request**: `BorrowerStep1RequestDto`
  - `Employer` (string, required)
  - `FirstName` (string, required)
  - `LastName` (string, required)
  - `Email` (email, required)
  - `CompanyId` (Guid, required)
  - `ProductId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - Message: "Email OTP sent successfully"

##### Step 1B: Email OTP Validation
- **Endpoint**: `POST /api/borrower/step1b`
- **Request**: `BorrowerStep1BRequestDto`
  - `Otp` (string, required)
  - `LoanId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - Message: "Email validated successfully"

##### Step 2: Bank and BVN Information
- **Endpoint**: `POST /api/borrower/step2`
- **Request**: `BorrowerStep2RequestDto`
  - `Bank` (string, required)
  - `AccountNo` (string, required)
  - `BVN` (string, required)
  - `LoanId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - Message: "BVN validated successfully"

##### Step 2B: BVN OTP Validation
- **Endpoint**: `POST /api/borrower/step2b`
- **Request**: `BorrowerStep2BRequestDto`
  - `Otp` (string, required)
  - `LoanId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - Message: "BVN validation successful"

##### Step 3: Address and Document Upload
- **Endpoint**: `POST /api/borrower/step3`
- **Request**: `BorrowerStep3RequestDto`
  - `Address` (string, required)
  - `IdNumber` (string, required)
  - `FrontImageBase64` (string, required)
  - `BackImageBase64` (string, required)
  - `LoanId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - `maxLoanEligible` (decimal)
  - `minLoanEligible` (decimal)
  - `maxTenor` (int)
  - `minTenor` (int)
  - Message: "Information added successfully"

##### Step 4: Final Loan Submission
- **Endpoint**: `POST /api/borrower/step4`
- **Request**: `BorrowerStep4RequestDto`
  - `LoanAmount` (decimal, required)
  - `Tenor` (int, required)
  - `LoanId` (Guid, required)
- **Response**: 
  - `loanId` (Guid)
  - `repaymentAmount` (decimal)
  - `tenor` (int)
  - `monthlyRepaymentAmount` (decimal)
  - Message: "Loan submitted successfully"

#### 2. **Standalone OTP Endpoints**

##### Generate Email OTP
- **Endpoint**: `POST /api/borrower/generate-email-otp`
- **Request**: `GenerateEmailOtpRequestDto`
  - `Email` (email, required)
- **Response**: Message: "Email OTP sent successfully"

##### Validate Email OTP
- **Endpoint**: `POST /api/borrower/validate-email-otp`
- **Request**: `ValidateEmailOtpRequestDto`
  - `Email` (email, required)
  - `Otp` (string, required)
- **Response**: Message: "OTP validation successful"

##### Generate BVN OTP
- **Endpoint**: `POST /api/borrower/generate-bvn-otp`
- **Request**: `GenerateBvnOtpRequestDto`
  - `BVN` (string, required)
- **Response**: Message: "BVN OTP sent successfully"

##### Validate BVN OTP
- **Endpoint**: `POST /api/borrower/validate-bvn-otp`
- **Request**: `ValidateBvnOtpRequestDto`
  - `BVN` (string, required)
  - `Otp` (string, required)
- **Response**: Message: "BVN validation successful"

#### 3. **New Models and Data Structures**

##### BorrowerApplication Model
```csharp
public class BorrowerApplication : Base
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Employer { get; set; }
    
    // Bank and BVN information (Step 2)
    public string? Bank { get; set; }
    public string? AccountNo { get; set; }
    public string? BVN { get; set; }
    
    // Address and documents (Step 3)
    public string? Address { get; set; }
    public string? IdNumber { get; set; }
    public Guid? FrontDocumentId { get; set; }
    public Guid? BackDocumentId { get; set; }
    
    // Loan information
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    
    public Guid ProductId { get; set; }
    public LoanProduct Product { get; set; } = default!;
    
    public Guid? LoanId { get; set; }
    public Loan? Loan { get; set; }
    
    // Eligibility (calculated in Step 3)
    public decimal? MaxLoanEligible { get; set; }
    public decimal? MinLoanEligible { get; set; }
    public int? MaxTenor { get; set; }
    public int? MinTenor { get; set; }
    
    // Tracking
    public BorrowerOnboardingStep CurrentStep { get; set; }
    public string? EmailOtp { get; set; }
    public DateTime? EmailOtpGeneratedAt { get; set; }
    public string? BvnOtp { get; set; }
    public DateTime? BvnOtpGeneratedAt { get; set; }
    public DateTime? DocumentsUploadedAt { get; set; }
    public DateTime? LoanSubmittedAt { get; set; }
}
```

##### BorrowerOnboardingStep Enum
```csharp
public enum BorrowerOnboardingStep
{
    Step1_EmailSent = 1,
    Step1B_EmailValidated = 2,
    Step2_BvnSubmitted = 3,
    Step2B_BvnValidated = 4,
    Step3_DocumentsUploaded = 5,
    Step4_LoanSubmitted = 6
}
```

#### 4. **Services and Repositories**

##### IBorrowerOnboardingService
- Complete service interface with all 4 steps and OTP methods
- Handles step validation and state management
- Integrates with document upload service
- Calculates loan eligibility based on loan product

##### BorrowerOnboardingService Implementation
- Full implementation with error handling
- Step progression validation
- OTP generation and validation (mock implementation)
- Document upload integration
- Loan creation and repayment calculation

##### IBorrowerApplicationRepository
- Repository interface for BorrowerApplication CRUD operations
- Methods for finding applications by ID, email, etc.

##### BorrowerApplicationRepository Implementation
- Entity Framework implementation
- All required CRUD operations

#### 5. **Legacy Endpoint Removal**
All legacy borrower endpoints have been removed and replaced with the new step-by-step flow:
- ❌ Removed: `POST /onboarding`
- ❌ Removed: `POST /verify-otp`
- ❌ Removed: `POST /save-personal-details`
- ❌ Removed: `POST /salary-history-review/{loanID}`
- ❌ Removed: `GET /loan-products/{companyId}`
- ❌ Removed: `POST /submit/{loanId}`
- ❌ Removed: `POST /validate/mandate/{loanId}`
- ❌ Removed: `POST /login`

### Technical Implementation Details

#### 1. **Database Changes**
- Added `BorrowerApplications` table to ApplicationDbContext
- Created migration for the new table structure
- No massive table reconstructions - new table with proper defaults

#### 2. **Dependency Injection Registration**
- Registered `IBorrowerOnboardingService` -> `BorrowerOnboardingService`
- Registered `IBorrowerApplicationRepository` -> `BorrowerApplicationRepository`

#### 3. **Error Handling**
- Comprehensive exception handling in all endpoints
- Proper HTTP status codes
- Consistent error response format using `ApiResponse.Fail()`

#### 4. **Validation**
- Step progression validation (can't skip steps)
- Input validation using data annotations
- Business rule validation (loan amounts, tenors)

#### 5. **Integration Points**
- **Document Service**: For uploading ID documents
- **Loan Product Service**: For fetching product details and calculations
- **Loan Service**: For creating final loan records
- **Email Service**: For OTP delivery (mock implementation ready)

### Mock Implementations
The following are currently mock implementations that can be replaced with real integrations:

1. **Email OTP Service**: Logs OTP to console instead of sending emails
2. **BVN OTP Service**: Logs OTP to console instead of BVN provider integration
3. **Email Delivery**: Ready for SMTP or email service integration

### Benefits of New Implementation

1. **Step-by-Step Flow**: Clear progression through onboarding
2. **State Management**: Tracks progress through each step
3. **Validation**: Ensures data integrity at each step
4. **Document Integration**: Seamless ID document upload
5. **Loan Eligibility**: Automatic calculation based on loan product
6. **Clean API**: RESTful endpoints with clear responsibilities
7. **Error Handling**: Comprehensive error management
8. **Extensible**: Easy to add new steps or modify existing ones

### Testing Recommendations

1. **Step Progression**: Test that steps must be completed in order
2. **OTP Validation**: Test OTP generation and validation logic
3. **Document Upload**: Test document upload and storage
4. **Loan Calculation**: Test loan eligibility and repayment calculations
5. **Error Scenarios**: Test invalid inputs and business rule violations
6. **Data Persistence**: Test that data is properly saved at each step

### Next Steps for Production

1. **Replace Mock OTP Services**: Integrate with real email and BVN OTP providers
2. **Add Email Templates**: Create professional email templates for OTP delivery
3. **Add Rate Limiting**: Implement rate limiting for OTP generation
4. **Add Monitoring**: Add logging and monitoring for the onboarding flow
5. **Security Review**: Review security implications of storing sensitive data
6. **Performance Testing**: Test performance with high volume of applications

## Integration Guide

### For Frontend Developers

The new API provides a clear 4-step onboarding flow:

1. **Step 1**: Collect basic info → Get loan ID
2. **Step 1B**: Validate email OTP → Continue to Step 2
3. **Step 2**: Collect bank/BVN → Get BVN OTP
4. **Step 2B**: Validate BVN OTP → Continue to Step 3
5. **Step 3**: Upload documents → Get loan eligibility limits
6. **Step 4**: Submit final loan → Get repayment details

Each step returns the `loanId` which should be passed to subsequent steps.

### For Backend Integration

The service layer is designed to be easily extended:
- Add new validation rules in the service layer
- Integrate with external services (email, BVN providers)
- Modify loan eligibility calculations
- Add new onboarding steps if needed

---

## ✅ Implementation Complete

All borrower onboarding endpoints have been successfully refactored and implemented according to the requirements. The system now provides a clean, step-by-step onboarding flow with proper validation, state management, and integration capabilities.
