using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IBorrowerOnboardingService
{
    // Step 1: Initial borrower information
    Task<BorrowerStep1ResponseDto> Step1_SaveBorrowerInfoAsync(BorrowerStep1RequestDto request);
    
    // Step 1B: Email OTP validation
    Task<BorrowerStep1BResponseDto> Step1B_ValidateEmailOtpAsync(BorrowerStep1BRequestDto request);
    
    // Step 2: Bank info, Address and documents
    Task<BorrowerStep2ResponseDto> Step2_SaveBankAddressDocumentsAsync(BorrowerStep2RequestDto request);
    
    // Step 3: Loan application
    Task<BorrowerStep3ResponseDto> Step3_SubmitLoanApplicationAsync(BorrowerStep3RequestDto request);
    
    // OTP services
    Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request);
    Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request);
    
    // Resend OTP services
    Task<ResendStep1EmailOtpResponseDto> ResendStep1EmailOtpAsync(ResendStep1EmailOtpRequestDto request);
    
    // Current step tracking
    Task<BorrowerCurrentStepResponseDto> GetCurrentStepAsync(BorrowerCurrentStepRequestDto request);
    
    // Update documents (works regardless of completion status)
    Task<UpdateDocumentsResponseDto> UpdateDocumentsAsync(UpdateDocumentsRequestDto request);
    
    // Request borrower to re-upload images
    Task<RequestImageReuploadResponseDto> RequestImageReuploadAsync(RequestImageReuploadDto request);
}
