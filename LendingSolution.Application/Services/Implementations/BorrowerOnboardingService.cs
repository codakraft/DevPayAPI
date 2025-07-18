using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

public class BorrowerOnboardingService : IBorrowerOnboardingService
{
    private readonly IBorrowerApplicationRepository _borrowerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoanProductRepository _loanProductRepository;
    private readonly IDocumentService _documentService;
    private readonly ILoanRepository _loanRepository;
    private readonly ILogger<BorrowerOnboardingService> _logger;

    public BorrowerOnboardingService(
        IBorrowerApplicationRepository borrowerRepository,
        ICompanyRepository companyRepository,
        ILoanProductRepository loanProductRepository,
        IDocumentService documentService,
        ILoanRepository loanRepository,
        ILogger<BorrowerOnboardingService> logger)
    {
        _borrowerRepository = borrowerRepository;
        _companyRepository = companyRepository;
        _loanProductRepository = loanProductRepository;
        _documentService = documentService;
        _loanRepository = loanRepository;
        _logger = logger;
    }

    public async Task<BorrowerStep1ResponseDto> Step1_SaveBorrowerInfoAsync(BorrowerStep1RequestDto request)
    {
        try
        {
            // Check if borrower already exists with this email
            var existingApplication = await _borrowerRepository.GetByEmailAsync(request.Email);
            if (existingApplication != null && !existingApplication.IsCompleted)
            {
                throw new AppException("An active application already exists for this email address", 409);
            }

            // Validate company exists
            var company = await _companyRepository.GetCompanyById(request.CompanyId);
            if (company == null)
            {
                throw new AppException("Company not found", 404);
            }

            // Validate loan product exists
            var product = await _loanProductRepository.GetLoanProductById(request.ProductId);
            if (product == null)
            {
                throw new AppException("Loan product not found", 404);
            }

            // Create borrower application
            var application = new BorrowerApplication
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Employer = request.Employer,
                CompanyId = request.CompanyId,
                ProductId = request.ProductId,
                CurrentStep = BorrowerOnboardingStep.Step1_EmailSent
            };

            var createdApplication = await _borrowerRepository.CreateAsync(application);

            // Generate and send email OTP
            await GenerateEmailOtpAsync(new GenerateEmailOtpRequestDto { EmailAddress = request.Email });

            _logger.LogInformation("Step 1 completed for borrower {Email}, Application ID: {ApplicationId}", 
                request.Email, createdApplication.Id);

            return new BorrowerStep1ResponseDto
            {
                LoanId = createdApplication.Id // Using application ID as loanId for now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 1 for email {Email}", request.Email);
            throw;
        }
    }

    public async Task<BorrowerStep1BResponseDto> Step1B_ValidateEmailOtpAsync(BorrowerStep1BRequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
            if (application == null)
            {
                throw new AppException("Application not found", 404);
            }

            if (application.CurrentStep != BorrowerOnboardingStep.Step1_EmailSent)
            {
                throw new AppException("Invalid step for this operation", 400);
            }

            // Validate OTP (mock implementation - replace with real OTP validation)
            if (!ValidateOtp(application.Email, request.Otp, application.LastEmailOtp, application.EmailOtpGeneratedAt))
            {
                throw new AppException("Invalid or expired OTP", 400);
            }

            // Update application
            application.CurrentStep = BorrowerOnboardingStep.Step1B_EmailValidated;
            application.EmailVerifiedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _borrowerRepository.UpdateAsync(application);

            _logger.LogInformation("Step 1B completed for application {ApplicationId}", request.LoanId);

            return new BorrowerStep1BResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 1B for application {LoanId}", request.LoanId);
            throw;
        }
    }

    public async Task<BorrowerStep2ResponseDto> Step2_SaveBankBvnInfoAsync(BorrowerStep2RequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
            if (application == null)
            {
                throw new AppException("Application not found", 404);
            }

            if (application.CurrentStep != BorrowerOnboardingStep.Step1B_EmailValidated)
            {
                throw new AppException("Invalid step for this operation", 400);
            }

            // Update application with bank and BVN info
            application.Bank = request.Bank;
            application.AccountNo = request.AccountNo;
            application.BVN = request.BVN;
            application.CurrentStep = BorrowerOnboardingStep.Step2_BvnSubmitted;
            application.UpdatedAt = DateTime.UtcNow;

            await _borrowerRepository.UpdateAsync(application);

            // Generate BVN OTP
            await GenerateBvnOtpAsync(new GenerateBvnOtpRequestDto { BVN = request.BVN });

            _logger.LogInformation("Step 2 completed for application {ApplicationId}", request.LoanId);

            return new BorrowerStep2ResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 2 for application {LoanId}", request.LoanId);
            throw;
        }
    }

    public async Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
            if (application == null)
            {
                throw new AppException("Application not found", 404);
            }

            if (application.CurrentStep != BorrowerOnboardingStep.Step2_BvnSubmitted)
            {
                throw new AppException("Invalid step for this operation", 400);
            }

            // Validate BVN OTP (mock implementation)
            if (!ValidateOtp(application.BVN!, request.Otp, application.LastBvnOtp, application.BvnOtpGeneratedAt))
            {
                throw new AppException("Invalid or expired BVN OTP", 400);
            }

            // Update application
            application.CurrentStep = BorrowerOnboardingStep.Step2B_BvnValidated;
            application.BvnVerifiedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _borrowerRepository.UpdateAsync(application);

            _logger.LogInformation("Step 2B completed for application {ApplicationId}", request.LoanId);

            return new BorrowerStep2BResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 2B for application {LoanId}", request.LoanId);
            throw;
        }
    }

    public async Task<BorrowerStep3ResponseDto> Step3_SaveAddressDocumentsAsync(BorrowerStep3RequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
            if (application == null)
            {
                throw new AppException("Application not found", 404);
            }

            if (application.CurrentStep != BorrowerOnboardingStep.Step2B_BvnValidated)
            {
                throw new AppException("Invalid step for this operation", 400);
            }

            // Upload documents
            var frontDocResult = await _documentService.UploadDocumentAsync(new UploadDocumentDto
            {
                DocumentName = $"ID_Front_{application.Email}",
                Base64String = request.FrontImageBase64
            }, application.Id.ToString());

            var backDocResult = await _documentService.UploadDocumentAsync(new UploadDocumentDto
            {
                DocumentName = $"ID_Back_{application.Email}",
                Base64String = request.BackImageBase64
            }, application.Id.ToString());

            // Calculate loan eligibility (mock calculation based on product)
            var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId);
            
            if (product == null)
            {
                throw new AppException("Loan product not found", 404);
            }
            
            var minLoanEligible = product.MinAmount;
            var maxLoanEligible = product.MaxAmount * 0.8m; // 80% of max as example
            var minTenor = product.MinTenor;
            var maxTenor = product.MaxTenor;

            // Update application
            application.Address = request.Address;
            application.IdNumber = request.IdNumber;
            application.FrontDocumentId = Guid.TryParse(frontDocResult.DocumentId, out var frontDocId) ? frontDocId : (Guid?)null;
            application.BackDocumentId = Guid.TryParse(backDocResult.DocumentId, out var backDocId) ? backDocId : (Guid?)null;
            application.MinLoanEligible = minLoanEligible;
            application.MaxLoanEligible = maxLoanEligible;
            application.MinTenor = minTenor;
            application.MaxTenor = maxTenor;
            application.CurrentStep = BorrowerOnboardingStep.Step3_DocumentsUploaded;
            application.DocumentsUploadedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            await _borrowerRepository.UpdateAsync(application);

            _logger.LogInformation("Step 3 completed for application {ApplicationId}", request.LoanId);

            return new BorrowerStep3ResponseDto
            {
                MinLoanEligible = minLoanEligible,
                MaxLoanEligible = maxLoanEligible,
                MinTenor = minTenor,
                MaxTenor = maxTenor
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 3 for application {LoanId}", request.LoanId);
            throw;
        }
    }

    public async Task<BorrowerStep4ResponseDto> Step4_SubmitLoanApplicationAsync(BorrowerStep4RequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
            if (application == null)
            {
                throw new AppException("Application not found", 404);
            }

            if (application.CurrentStep != BorrowerOnboardingStep.Step3_DocumentsUploaded)
            {
                throw new AppException("Invalid step for this operation", 400);
            }

            // Validate loan amount and tenor are within eligible range
            if (request.LoanAmount < application.MinLoanEligible || request.LoanAmount > application.MaxLoanEligible)
            {
                throw new AppException($"Loan amount must be between {application.MinLoanEligible:C} and {application.MaxLoanEligible:C}", 400);
            }

            if (request.Tenor < application.MinTenor || request.Tenor > application.MaxTenor)
            {
                throw new AppException($"Tenor must be between {application.MinTenor} and {application.MaxTenor} months", 400);
            }

            // Calculate repayment (mock calculation)
            var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId);
            
            if (product == null)
            {
                throw new AppException("Loan product not found", 404);
            }
            
            var monthlyInterestRate = product.InterestRate / 100 / 12;
            var totalRepayment = request.LoanAmount * (1 + (monthlyInterestRate * request.Tenor));
            var monthlyRepayment = totalRepayment / request.Tenor;

            // Create actual loan record
            var loan = new Loan
            {
                UserId = application.Email, // Temporary - should create actual user
                Amount = request.LoanAmount,
                DurationInMonths = request.Tenor,
                Purpose = "Personal Loan", // Default purpose
                Status = LoanStatus.Pending,
                CompanyId = application.CompanyId,
                ProductId = application.ProductId,
                AccountId = Guid.NewGuid(), // Temporary - should create actual account
                Message = "Loan application submitted"
            };

            // Note: This would normally create the loan in the loans table
            // For now, we'll just update the application

            // Update application
            application.CurrentStep = BorrowerOnboardingStep.Step4_LoanSubmitted;
            application.LoanSubmittedAt = DateTime.UtcNow;
            application.IsCompleted = true;
            application.UpdatedAt = DateTime.UtcNow;

            await _borrowerRepository.UpdateAsync(application);

            // Send email notification (mock)
            _logger.LogInformation("Loan application submitted for {Email}, Amount: {Amount}, Tenor: {Tenor}", 
                application.Email, request.LoanAmount, request.Tenor);

            return new BorrowerStep4ResponseDto
            {
                RepaymentAmount = totalRepayment,
                Tenor = request.Tenor,
                MonthlyRepaymentAmount = monthlyRepayment
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Step 4 for application {LoanId}", request.LoanId);
            throw;
        }
    }

    public async Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request)
    {
        try
        {
            // Generate OTP (mock implementation)
            var otp = GenerateOtp();
            
            // Find application by email and update OTP
            var application = await _borrowerRepository.GetByEmailAsync(request.EmailAddress);
            if (application != null)
            {
                application.LastEmailOtp = otp;
                application.EmailOtpGeneratedAt = DateTime.UtcNow;
                await _borrowerRepository.UpdateAsync(application);
            }

            // Send email (mock implementation)
            _logger.LogInformation("Email OTP generated for {Email}: {OTP}", request.EmailAddress, otp);

            return new GenerateEmailOtpResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email OTP for {Email}", request.EmailAddress);
            throw;
        }
    }

    public async Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request)
    {
        try
        {
            var application = await _borrowerRepository.GetByEmailAsync(request.Email);
            if (application == null)
            {
                throw new AppException("No application found for this email", 404);
            }

            if (!ValidateOtp(request.Email, request.Otp, application.LastEmailOtp, application.EmailOtpGeneratedAt))
            {
                throw new AppException("Invalid or expired OTP", 400);
            }

            return new ValidateEmailOtpResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating email OTP for {Email}", request.Email);
            throw;
        }
    }

    public Task<GenerateBvnOtpResponseDto> GenerateBvnOtpAsync(GenerateBvnOtpRequestDto request)
    {
        try
        {
            // Generate BVN OTP (mock implementation)
            var otp = GenerateOtp();
            
            _logger.LogInformation("BVN OTP generated for BVN {BVN}: {OTP}", request.BVN.Substring(0, 3) + "***", otp);

            return Task.FromResult(new GenerateBvnOtpResponseDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating BVN OTP for BVN {BVN}", request.BVN);
            throw;
        }
    }

    public Task<ValidateBvnOtpResponseDto> ValidateBvnOtpAsync(ValidateBvnOtpRequestDto request)
    {
        try
        {
            // Mock BVN OTP validation
            _logger.LogInformation("BVN OTP validated for BVN {BVN}", request.BVN.Substring(0, 3) + "***");

            return Task.FromResult(new ValidateBvnOtpResponseDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating BVN OTP for BVN {BVN}", request.BVN);
            throw;
        }
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private bool ValidateOtp(string identifier, string providedOtp, string? storedOtp, DateTime? generatedAt)
    {
        if (string.IsNullOrEmpty(storedOtp) || !generatedAt.HasValue)
        {
            return false;
        }

        // OTP expires after 10 minutes
        if (DateTime.UtcNow.Subtract(generatedAt.Value).TotalMinutes > 10)
        {
            return false;
        }

        return providedOtp == storedOtp;
    }
}
