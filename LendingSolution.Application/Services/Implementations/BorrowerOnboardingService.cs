using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Implementations;

public class BorrowerOnboardingService : IBorrowerOnboardingService
{
    private readonly IBorrowerApplicationRepository _borrowerRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILoanProductRepository _loanProductRepository;
    private readonly IDocumentService _documentService;
    private readonly ILoanRepository _loanRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public BorrowerOnboardingService(
        IBorrowerApplicationRepository borrowerRepository,
        ICompanyRepository companyRepository,
        ILoanProductRepository loanProductRepository,
        IDocumentService documentService,
        ILoanRepository loanRepository,
        IEmailService emailService,
        ISmsService smsService)
    {
        _borrowerRepository = borrowerRepository;
        _companyRepository = companyRepository;
        _loanProductRepository = loanProductRepository;
        _documentService = documentService;
        _loanRepository = loanRepository;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<BorrowerStep1ResponseDto> Step1_SaveBorrowerInfoAsync(BorrowerStep1RequestDto request)
    {
        // Check if borrower already exists with this email
        var existingApplication = await _borrowerRepository.GetByEmailAsync(request.Email);
        if (existingApplication != null && !existingApplication.IsCompleted)
        {
            throw new AppException("An active application already exists for this email address", 409);
        }

        // Validate loan product exists and get company from product
        var product = await _loanProductRepository.GetLoanProductById(request.ProductId);
        if (product == null)
        {
            throw new AppException("Loan product not found", 404);
        }

        // Get company from the product
        var company = await _companyRepository.GetCompanyById(product.CompanyId);
        if (company == null)
        {
            throw new AppException("Company associated with this loan product not found", 404);
        }

        // Create borrower application
        var application = new BorrowerApplication
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Employer = request.Employer,
            CompanyId = product.CompanyId, // Get CompanyId from the product
            ProductId = request.ProductId,
            CurrentStep = BorrowerOnboardingStep.Step1_EmailSent
        };

        var createdApplication = await _borrowerRepository.CreateAsync(application);

        // Generate and send email OTP
        await GenerateEmailOtpAsync(new GenerateEmailOtpRequestDto { EmailAddress = request.Email });

        return new BorrowerStep1ResponseDto
        {
            LoanId = createdApplication.Id // Using application ID as loanId for now
        };
    }

    public async Task<BorrowerStep1BResponseDto> Step1B_ValidateEmailOtpAsync(BorrowerStep1BRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

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

        return new BorrowerStep1BResponseDto();
    }

    public async Task<BorrowerStep2ResponseDto> Step2_SaveBankBvnInfoAsync(BorrowerStep2RequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        if (application.CurrentStep != BorrowerOnboardingStep.Step1B_EmailValidated)
        {
            throw new AppException("Invalid step for this operation", 400);
        }

        // Update application with bank and BVN info
        application.BankCode = request.BankCode;
        application.AccountNo = request.AccountNo;
        application.BVN = request.BVN;
        application.CurrentStep = BorrowerOnboardingStep.Step2_BvnSubmitted;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        // Generate BVN OTP
        await GenerateBvnOtpAsync(new GenerateBvnOtpRequestDto { BVN = request.BVN });

        return new BorrowerStep2ResponseDto();
    }

    public async Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request)
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

        return new BorrowerStep2BResponseDto();
    }

    public async Task<BorrowerStep3ResponseDto> Step3_SaveAddressDocumentsAsync(BorrowerStep3RequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        if (application.CurrentStep != BorrowerOnboardingStep.Step2B_BvnValidated)
        {
            throw new AppException("Invalid step for this operation", 400);
        }

        // Upload documents
        var frontDocResult = await _documentService.UploadDocumentAsync(new UploadDocumentDto
        {
            DocumentName = $"ID_Front_{application.Email}",
            Base64String = request.FrontImageBase64,
            FileExtension = request.FrontImageExtension

        }, application.Id.ToString());

        var backDocResult = await _documentService.UploadDocumentAsync(new UploadDocumentDto
        {
            DocumentName = $"ID_Back_{application.Email}",
            Base64String = request.BackImageBase64,
            FileExtension = request.BackImageExtension
        }, application.Id.ToString());

        // Calculate loan eligibility (mock calculation based on product)
        var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId) ?? throw new AppException("Loan product not found", 404);

        var minLoanEligible = product.MinAmount;
        var maxLoanEligible = product.MaxAmount; // 80% of max as example
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

        return new BorrowerStep3ResponseDto
        {
            MinLoanEligible = minLoanEligible,
            MaxLoanEligible = maxLoanEligible,
            MinTenor = minTenor,
            MaxTenor = maxTenor
        };
    }

    public async Task<BorrowerStep4ResponseDto> Step4_SubmitLoanApplicationAsync(BorrowerStep4RequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

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
        var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId) ?? throw new AppException("Loan product not found", 404);


        var monthlyInterestRate = product.InterestRate / 100 / 12;
        var totalRepayment = request.LoanAmount * (1 + (monthlyInterestRate * request.Tenor));
        var monthlyRepayment = totalRepayment / request.Tenor;

        // Create actual loan record
        var loan = new Loan
        {
            Amount = request.LoanAmount,
            DurationInMonths = request.Tenor,
            Status = LoanStatus.Pending,
            CompanyId = application.CompanyId,
            ProductId = application.ProductId
        };

        // Create the loan
        var loanCreated = await _loanRepository.CreateLoan(loan);
        if (!loanCreated)
        {
            throw new AppException("Failed to create loan", 500);
        }
        
        // Since the loan entity now has the generated ID, we can use it
        application.LoanId = loan.Id;

        // Update application
        application.CurrentStep = BorrowerOnboardingStep.Step4_LoanSubmitted;
        application.LoanSubmittedAt = DateTime.UtcNow;
        application.IsCompleted = true;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        // Send email notification (mock)

        return new BorrowerStep4ResponseDto
        {
            RepaymentAmount = totalRepayment,
            Tenor = request.Tenor,
            MonthlyRepaymentAmount = monthlyRepayment
        };
    }

    public async Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request)
    {
        // Generate OTP
        var otp = GenerateOtp();

        // Find application by email and update OTP
        var application = await _borrowerRepository.GetByEmailAsync(request.EmailAddress);
        if (application != null)
        {
            application.LastEmailOtp = otp;
            application.EmailOtpGeneratedAt = DateTime.UtcNow;
            await _borrowerRepository.UpdateAsync(application);

            // Send actual email OTP
            var emailSent = await _emailService.SendOtpEmailAsync(
                request.EmailAddress,
                otp,
                "Email Verification"
            );

            if (!emailSent)
            {
                throw new AppException("Failed to send verification email. Please try again.", 500);
            }
        }
        else
        {
            throw new AppException("No application found for this email", 404);
        }

        return new GenerateEmailOtpResponseDto();
    }

    public async Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request)
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

    public async Task<GenerateBvnOtpResponseDto> GenerateBvnOtpAsync(GenerateBvnOtpRequestDto request)
    {
        // Generate BVN OTP
        var otp = GenerateOtp();

        // Find application by BVN and update OTP
        var application = await _borrowerRepository.GetByBvnAsync(request.BVN);
        if (application != null)
        {
            application.LastBvnOtp = otp;
            application.BvnOtpGeneratedAt = DateTime.UtcNow;
            await _borrowerRepository.UpdateAsync(application);

            // Note: In a real implementation, BVN OTP would be sent via the bank's SMS service
            // or retrieved from a BVN verification service like Mono, Paystack, or Flutterwave
            // For now, we simulate the OTP generation and logging

            // In production, this would be:
            // 1. Call BVN verification service to get phone number
            // 2. Send OTP via that phone number
            // 3. The OTP validation would also go through the BVN service

            // Simulate SMS sending (replace with actual BVN service integration)
            var phoneNumber = "0901234567"; // This would come from BVN service
            var smsSent = await _smsService.SendOtpSmsAsync(phoneNumber, otp, "BVN Verification");

            if (!smsSent)
            {
                // In development, this might be expected if SMS is not configured
                // In production, this should be a critical error
                throw new AppException("Failed to send BVN verification SMS. Please try again.", 500);
            }
        }
        else
        {
            throw new AppException("No application found for this BVN", 404);
        }

        return new GenerateBvnOtpResponseDto();
    }

    public Task<ValidateBvnOtpResponseDto> ValidateBvnOtpAsync(ValidateBvnOtpRequestDto request)
    {
        return Task.FromResult(new ValidateBvnOtpResponseDto());
    }

    public async Task<ResendStep1EmailOtpResponseDto> ResendStep1EmailOtpAsync(ResendStep1EmailOtpRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId);
        if (application == null)
        {
            throw new AppException("Application not found", 404);
        }

        // Only allow resending if the application is still on Step 1 (email sent but not validated)
        if (application.CurrentStep != BorrowerOnboardingStep.Step1_EmailSent)
        {
            throw new AppException("Email OTP can only be resent for applications in Step 1 (Email Sent) status", 400);
        }

        // Generate new OTP
        var otp = GenerateOtp();

        // Update application with new OTP
        application.LastEmailOtp = otp;
        application.EmailOtpGeneratedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        // Send actual email OTP
        var emailSent = await _emailService.SendOtpEmailAsync(
            application.Email,
            otp,
            "Email Verification - Resent"
        );

        if (!emailSent)
        {
            throw new AppException("Failed to resend verification email. Please try again.", 500);
        }

        return new ResendStep1EmailOtpResponseDto();
    }

    private string GenerateOtp()
    {
        // var random = new Random();
        // return random.Next(100000, 999999).ToString();
        return "564312";
    }

    private bool ValidateOtp(string identifier, string providedOtp, string? storedOtp, DateTime? generatedAt)
    {
        if (string.IsNullOrEmpty(storedOtp) || !generatedAt.HasValue)
        {
            return false;
        }

        // // OTP expires after 10 minutes
        // if (DateTime.UtcNow.Subtract(generatedAt.Value).TotalMinutes > 10)
        // {
        //     return false;
        // }

        return providedOtp == storedOtp;
    }

    public async Task<BorrowerCurrentStepResponseDto> GetCurrentStepAsync(BorrowerCurrentStepRequestDto request)
    {
        var borrowerApplication = await _borrowerRepository.GetByLoanIdAsync(request.LoanId);
        if (borrowerApplication == null)
        {
            throw new AppException("Borrower application not found", 404);
        }

        var response = new BorrowerCurrentStepResponseDto
        {
            LoanId = borrowerApplication.LoanId ?? Guid.Empty,
            Email = borrowerApplication.Email,
            FirstName = borrowerApplication.FirstName,
            LastName = borrowerApplication.LastName,
            CurrentStep = borrowerApplication.CurrentStep,
            CurrentStepName = GetStepName(borrowerApplication.CurrentStep),
            CurrentStepDescription = GetStepDescription(borrowerApplication.CurrentStep),
            IsCompleted = borrowerApplication.IsCompleted,
            CreatedAt = borrowerApplication.CreatedAt,
            UpdatedAt = borrowerApplication.UpdatedAt,
            StepNumber = GetStepNumber(borrowerApplication.CurrentStep),
            TotalSteps = 6,
            CompanyName = borrowerApplication.Company?.Name ?? "Unknown Company",
            ProductName = borrowerApplication.Product?.Name ?? "Unknown Product",
            RequiredActions = GetRequiredActions(borrowerApplication.CurrentStep)
        };

        // Calculate progress percentage
        response.ProgressPercentage = Math.Round((decimal)response.StepNumber / response.TotalSteps * 100, 2);

        // Set next step information if not completed
        if (!borrowerApplication.IsCompleted)
        {
            var nextStep = GetNextStep(borrowerApplication.CurrentStep);
            if (nextStep.HasValue)
            {
                response.NextStepName = GetStepName(nextStep.Value);
                response.NextStepDescription = GetStepDescription(nextStep.Value);
            }
        }

        // Set completion timestamps based on current step
        if (borrowerApplication.CurrentStep >= BorrowerOnboardingStep.Step1B_EmailValidated || borrowerApplication.IsCompleted)
        {
            response.EmailVerifiedAt = borrowerApplication.EmailVerifiedAt;
        }
        
        if (borrowerApplication.CurrentStep >= BorrowerOnboardingStep.Step2B_BvnValidated || borrowerApplication.IsCompleted)
        {
            response.BvnVerifiedAt = borrowerApplication.BvnVerifiedAt;
        }
        
        if (borrowerApplication.CurrentStep >= BorrowerOnboardingStep.Step3_DocumentsUploaded || borrowerApplication.IsCompleted)
        {
            response.DocumentsUploadedAt = borrowerApplication.DocumentsUploadedAt;
        }
        
        if (borrowerApplication.CurrentStep >= BorrowerOnboardingStep.Step4_LoanSubmitted || borrowerApplication.IsCompleted)
        {
            response.LoanSubmittedAt = borrowerApplication.LoanSubmittedAt;
        }

        // Set eligibility information if available (after Step 3 completion)
        if (borrowerApplication.CurrentStep >= BorrowerOnboardingStep.Step3_DocumentsUploaded)
        {
            response.MaxLoanEligible = borrowerApplication.MaxLoanEligible;
            response.MinLoanEligible = borrowerApplication.MinLoanEligible;
            response.MaxTenor = borrowerApplication.MaxTenor;
            response.MinTenor = borrowerApplication.MinTenor;
        }

        return response;
    }

    private static string GetStepName(BorrowerOnboardingStep step)
    {
        return step switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => "Email Verification",
            BorrowerOnboardingStep.Step1B_EmailValidated => "Email Verified",
            BorrowerOnboardingStep.Step2_BvnSubmitted => "Bank & BVN Information",
            BorrowerOnboardingStep.Step2B_BvnValidated => "BVN Verified",
            BorrowerOnboardingStep.Step3_DocumentsUploaded => "Documents Upload",
            BorrowerOnboardingStep.Step4_LoanSubmitted => "Loan Application",
            _ => "Unknown Step"
        };
    }

    private static string GetStepDescription(BorrowerOnboardingStep step)
    {
        return step switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => "Please verify your email address by entering the OTP sent to your email.",
            BorrowerOnboardingStep.Step1B_EmailValidated => "Email verified successfully. Proceed to provide your bank and BVN information.",
            BorrowerOnboardingStep.Step2_BvnSubmitted => "Please verify your BVN by entering the OTP sent to your registered phone number.",
            BorrowerOnboardingStep.Step2B_BvnValidated => "BVN verified successfully. Please upload your required documents.",
            BorrowerOnboardingStep.Step3_DocumentsUploaded => "Documents uploaded successfully. You can now submit your loan application.",
            BorrowerOnboardingStep.Step4_LoanSubmitted => "Loan application submitted successfully. Your application is under review.",
            _ => "Unknown step description"
        };
    }

    private static int GetStepNumber(BorrowerOnboardingStep step)
    {
        return step switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => 1,
            BorrowerOnboardingStep.Step1B_EmailValidated => 2,
            BorrowerOnboardingStep.Step2_BvnSubmitted => 3,
            BorrowerOnboardingStep.Step2B_BvnValidated => 4,
            BorrowerOnboardingStep.Step3_DocumentsUploaded => 5,
            BorrowerOnboardingStep.Step4_LoanSubmitted => 6,
            _ => 0
        };
    }

    private static BorrowerOnboardingStep? GetNextStep(BorrowerOnboardingStep currentStep)
    {
        return currentStep switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => BorrowerOnboardingStep.Step1B_EmailValidated,
            BorrowerOnboardingStep.Step1B_EmailValidated => BorrowerOnboardingStep.Step2_BvnSubmitted,
            BorrowerOnboardingStep.Step2_BvnSubmitted => BorrowerOnboardingStep.Step2B_BvnValidated,
            BorrowerOnboardingStep.Step2B_BvnValidated => BorrowerOnboardingStep.Step3_DocumentsUploaded,
            BorrowerOnboardingStep.Step3_DocumentsUploaded => BorrowerOnboardingStep.Step4_LoanSubmitted,
            BorrowerOnboardingStep.Step4_LoanSubmitted => null, // Final step
            _ => null
        };
    }

    private static List<string> GetRequiredActions(BorrowerOnboardingStep step)
    {
        return step switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => new List<string> { "Verify your email address using the OTP sent to your email" },
            BorrowerOnboardingStep.Step1B_EmailValidated => new List<string> { "Provide your bank account details and BVN information" },
            BorrowerOnboardingStep.Step2_BvnSubmitted => new List<string> { "Verify your BVN using the OTP sent to your registered phone number" },
            BorrowerOnboardingStep.Step2B_BvnValidated => new List<string> { "Upload required documents (ID, utility bill, passport photo)" },
            BorrowerOnboardingStep.Step3_DocumentsUploaded => new List<string> { "Submit your loan application with desired amount and tenor" },
            BorrowerOnboardingStep.Step4_LoanSubmitted => new List<string> { "Your application is complete and under review" },
            _ => new List<string>()
        };
    }
}

