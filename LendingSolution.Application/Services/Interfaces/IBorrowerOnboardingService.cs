using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IBorrowerOnboardingService
{
    // Step 1: Initial borrower information
    Task<BorrowerStep1ResponseDto> Step1_SaveBorrowerInfoAsync(BorrowerStep1RequestDto request);
    
    // Step 1B: Email OTP validation
    Task<BorrowerStep1BResponseDto> Step1B_ValidateEmailOtpAsync(BorrowerStep1BRequestDto request);
    
    // Step 2: BVN submission
    Task<BorrowerStep2ResponseDto> Step2_SaveBvnAsync(BorrowerStep2RequestDto request);
    
    // Step 2B: BVN OTP validation
    Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request);
    
    // Step 3: Bank info, Address and documents
    Task<BorrowerStep3ResponseDto> Step3_SaveBankAddressDocumentsAsync(BorrowerStep3RequestDto request);
    
    // Step 4: Loan application
    Task<BorrowerStep4ResponseDto> Step4_SubmitLoanApplicationAsync(BorrowerStep4RequestDto request);

    // Step 5: Generate Direct Debit mandate
    Task<BorrowerStep5ResponseDto> Step5_GenerateMandateAsync(BorrowerStep5RequestDto request);

    // Step 6: Request OTP to initiate mandate activation
    Task<BorrowerStep6ResponseDto> Step6_RequestMandateOtpAsync(BorrowerStep6RequestDto request);

    // Step 6B: Validate OTP to activate the mandate (final onboarding step)
    Task<BorrowerStep6BResponseDto> Step6B_ActivateMandateAsync(BorrowerStep6BRequestDto request);
    
    // OTP services
    Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request);
    Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request);
    Task<GenerateBvnOtpResponseDto> GenerateBvnOtpAsync(GenerateBvnOtpRequestDto request);
    
    // Resend OTP services
    Task<ResendStep1EmailOtpResponseDto> ResendStep1EmailOtpAsync(ResendStep1EmailOtpRequestDto request);
    Task<ResendStep2BvnOtpResponseDto> ResendStep2BvnOtpAsync(ResendStep2BvnOtpRequestDto request);
    
    // Current step tracking
    Task<BorrowerCurrentStepResponseDto> GetCurrentStepAsync(BorrowerCurrentStepRequestDto request);
    
    // Update documents (works regardless of completion status)
    Task<UpdateDocumentsResponseDto> UpdateDocumentsAsync(UpdateDocumentsRequestDto request);
    
    // Request borrower to re-upload images
    Task<RequestImageReuploadResponseDto> RequestImageReuploadAsync(RequestImageReuploadDto request);
}
