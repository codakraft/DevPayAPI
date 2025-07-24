# Borrower Current Step API

This endpoint allows borrowers to retrieve their current onboarding step and progress information.

## Endpoint

**POST** `/api/borrower/current-step`

## Request Body

```json
{
  "loanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

## Response

```json
{
  "status": "Success",
  "message": "Current step retrieved successfully",
  "data": {
    "loanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "currentStep": 3,
    "currentStepName": "Bank & BVN Information",
    "currentStepDescription": "Please verify your BVN by entering the OTP sent to your registered phone number.",
    "nextStepName": "BVN Verified",
    "nextStepDescription": "BVN verified successfully. Please upload your required documents.",
    "isCompleted": false,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:30:00Z",
    "emailVerifiedAt": "2024-01-15T10:05:00Z",
    "bvnVerifiedAt": null,
    "documentsUploadedAt": null,
    "loanSubmittedAt": null,
    "stepNumber": 3,
    "totalSteps": 6,
    "progressPercentage": 50.00,
    "maxLoanEligible": null,
    "minLoanEligible": null,
    "maxTenor": null,
    "minTenor": null,
    "companyName": "Tech Solutions Ltd",
    "productName": "Employee Loan",
    "requiredActions": [
      "Verify your BVN using the OTP sent to your registered phone number"
    ]
  }
}
```

## Step Values and Descriptions

The onboarding process consists of 6 steps:

1. **Step1_EmailSent (1)**: Email Verification
   - Description: "Please verify your email address by entering the OTP sent to your email."
   - Required Action: Verify email address using OTP

2. **Step1B_EmailValidated (2)**: Email Verified
   - Description: "Email verified successfully. Proceed to provide your bank and BVN information."
   - Required Action: Provide bank account details and BVN information

3. **Step2_BvnSubmitted (3)**: Bank & BVN Information
   - Description: "Please verify your BVN by entering the OTP sent to your registered phone number."
   - Required Action: Verify BVN using OTP

4. **Step2B_BvnValidated (4)**: BVN Verified
   - Description: "BVN verified successfully. Please upload your required documents."
   - Required Action: Upload required documents (ID, utility bill, passport photo)

5. **Step3_DocumentsUploaded (5)**: Documents Upload
   - Description: "Documents uploaded successfully. You can now submit your loan application."
   - Required Action: Submit loan application with desired amount and tenor

6. **Step4_LoanSubmitted (6)**: Loan Application
   - Description: "Loan application submitted successfully. Your application is under review."
   - Required Action: Your application is complete and under review

## Response Fields

- **loanId**: Unique identifier for the loan application
- **email, firstName, lastName**: Borrower's personal information
- **currentStep**: Current step enum value (1-6)
- **currentStepName**: Human-readable name of the current step
- **currentStepDescription**: Description of what needs to be done in the current step
- **nextStepName, nextStepDescription**: Information about the next step (if not completed)
- **isCompleted**: Whether the entire onboarding process is completed
- **createdAt, updatedAt**: Timestamps for application creation and last update
- **emailVerifiedAt, bvnVerifiedAt, documentsUploadedAt, loanSubmittedAt**: Completion timestamps for each step
- **stepNumber, totalSteps**: Current step number out of total steps
- **progressPercentage**: Percentage of completion (0-100)
- **maxLoanEligible, minLoanEligible, maxTenor, minTenor**: Eligibility information (available after Step 3)
- **companyName, productName**: Company and loan product information
- **requiredActions**: Array of actions required to proceed to the next step

## Error Responses

### 404 - Borrower Application Not Found
```json
{
  "status": "Failed",
  "message": "Borrower application not found"
}
```

### 500 - Server Error
```json
{
  "status": "Failed",
  "message": "Something went wrong"
}
```

## Usage Example

```bash
curl -X POST "https://your-api-domain.com/api/borrower/current-step" \
  -H "Content-Type: application/json" \
  -d '{
    "loanId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

## Use Cases

1. **Progress Tracking**: Borrowers can check their current step and overall progress
2. **Step Validation**: Frontend applications can validate the current step before allowing certain actions
3. **UI State Management**: Applications can show appropriate UI components based on the current step
4. **Resume Process**: Borrowers can see where they left off and what actions are required
5. **Eligibility Display**: After Step 3, borrowers can see their loan eligibility information

## Integration Notes

- This endpoint should be called when a borrower logs in or accesses their application
- The response provides all necessary information to display a progress indicator
- The `requiredActions` array can be used to show specific instructions to the borrower
- Eligibility information is only populated after Step 3 (documents upload) is completed
