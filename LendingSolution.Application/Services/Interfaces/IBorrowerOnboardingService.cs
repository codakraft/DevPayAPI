using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IBorrowerOnboardingService
{
    // Step 1: Initial borrower information
    Task<BorrowerStep1ResponseDto> Step1_SaveBorrowerInfoAsync(BorrowerStep1RequestDto request);
    
    // Step 1B: Email OTP validation
    Task<BorrowerStep1BResponseDto> Step1B_ValidateEmailOtpAsync(BorrowerStep1BRequestDto request);
    
    // Step 2: Bank and BVN information
    Task<BorrowerStep2ResponseDto> Step2_SaveBankBvnInfoAsync(BorrowerStep2RequestDto request);
    
    // Step 2B: BVN OTP validation
    Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request);
    
    // Step 3: Address and documents
    Task<BorrowerStep3ResponseDto> Step3_SaveAddressDocumentsAsync(BorrowerStep3RequestDto request);
    
    // Step 4: Loan application
    Task<BorrowerStep4ResponseDto> Step4_SubmitLoanApplicationAsync(BorrowerStep4RequestDto request);
    
    // OTP services
    Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request);
    Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request);
    Task<GenerateBvnOtpResponseDto> GenerateBvnOtpAsync(GenerateBvnOtpRequestDto request);
    Task<ValidateBvnOtpResponseDto> ValidateBvnOtpAsync(ValidateBvnOtpRequestDto request);
    
    // Resend OTP services
    Task<ResendStep1EmailOtpResponseDto> ResendStep1EmailOtpAsync(ResendStep1EmailOtpRequestDto request);
    
    // Current step tracking
    Task<BorrowerCurrentStepResponseDto> GetCurrentStepAsync(BorrowerCurrentStepRequestDto request);
    
    // Update documents (works regardless of completion status)
    Task<UpdateDocumentsResponseDto> UpdateDocumentsAsync(UpdateDocumentsRequestDto request);
    
    // Request borrower to re-upload images
    Task<RequestImageReuploadResponseDto> RequestImageReuploadAsync(RequestImageReuploadDto request);
}
