using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

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
    private readonly IWalletService _walletService;
    private readonly ISettingsService _settingsService;
    private readonly IRemitaService _remitaService;
    private readonly IMonoService _monoService;
    private readonly ISalaryEligibilityService _salaryEligibilityService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BorrowerOnboardingService> _logger;

    public BorrowerOnboardingService(
        IBorrowerApplicationRepository borrowerRepository,
        ICompanyRepository companyRepository,
        ILoanProductRepository loanProductRepository,
        IDocumentService documentService,
        ILoanRepository loanRepository,
        IEmailService emailService,
        ISmsService smsService,
        IWalletService walletService,
        ISettingsService settingsService,
        IRemitaService remitaService,
        IMonoService monoService,
        ISalaryEligibilityService salaryEligibilityService,
        IConfiguration configuration,
        ILogger<BorrowerOnboardingService> logger)
    {
        _borrowerRepository = borrowerRepository;
        _companyRepository = companyRepository;
        _loanProductRepository = loanProductRepository;
        _documentService = documentService;
        _loanRepository = loanRepository;
        _emailService = emailService;
        _smsService = smsService;
        _walletService = walletService;
        _settingsService = settingsService;
        _remitaService = remitaService;
        _monoService = monoService;
        _salaryEligibilityService = salaryEligibilityService;
        _configuration = configuration;
        _logger = logger;
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
        var product = await _loanProductRepository.GetLoanProductById(request.ProductId) ?? throw new AppException("Loan product not found", 404);

        // Get company from the product
        var company = await _companyRepository.GetCompanyById(product.CompanyId) ?? throw new AppException("Company associated with this loan product not found", 404);

        // EARLY VALIDATION: Check company wallet balance before starting onboarding process
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            await _walletService.ValidateCompanyBalanceForFeeAsync(product.CompanyId, settings.OtpFee, "Email OTP");
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            _logger.LogWarning(
                "AUDIT: Borrower application blocked for Company {CompanyName} (ID: {CompanyId}). Reason: {Reason}",
                company.Name, product.CompanyId, ex.Message);
            throw new AppException(
                "We're unable to process your application at this time. Please contact support.",
                503);
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

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step1B_EmailValidated)
        {
            _logger.LogInformation("Borrower going back to Step 1B for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep2Onwards(application);
        }
        else if (application.CurrentStep < BorrowerOnboardingStep.Step1_EmailSent)
        {
            throw new AppException("Please complete Step 1 first", 400);
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

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Must have completed Step 1B at minimum
        if (application.CurrentStep < BorrowerOnboardingStep.Step1B_EmailValidated)
        {
            throw new AppException("Please complete email verification first", 400);
        }

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // Check active data provider
        var activeProvider = _configuration["ActiveDataProvider"] ?? "Remita";
        _logger.LogInformation("Active data provider for BVN verification: {Provider}", activeProvider);

        // Check company wallet balance before proceeding (both Remita OTP and Mono BVN lookup require fees)
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var feeDescription = activeProvider.Equals("Mono", StringComparison.OrdinalIgnoreCase) ? "Mono BVN Lookup" : "BVN OTP";
            await _walletService.ValidateCompanyBalanceForFeeAsync(application.CompanyId, settings.OtpFee, feeDescription);
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            _logger.LogWarning(
                "AUDIT: Borrower application blocked for Company {CompanyName} (ID: {CompanyId}). Reason: {Reason}",
                companyName, application.CompanyId, ex.Message);
            throw new AppException(
                "We're unable to process your application at this time. Please contact support.",
                503);
        }

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step2_BvnSubmitted)
        {
            _logger.LogInformation("Borrower going back to Step 2 for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep2BOnwards(application);
        }

        // Update application with bank and BVN info
        application.BankCode = request.BankCode;
        application.AccountNo = request.AccountNo;
        application.BVN = request.BVN;
        application.CurrentStep = BorrowerOnboardingStep.Step2_BvnSubmitted;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        // If Mono is active provider, use Mono BVN lookup directly (no OTP needed)
        if (activeProvider.Equals("Mono", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Using Mono BVN lookup for application {ApplicationId}", application.Id);
            
            try
            {
                var bvnLookupRequest = new MonoBvnLookupRequestDto
                {
                    Bvn = request.BVN
                };
                
                var bvnLookupResponse = await _monoService.BvnLookupAsync(bvnLookupRequest);
                
                if (bvnLookupResponse == null || bvnLookupResponse.Status != "successful")
                {
                    _logger.LogWarning("Mono BVN lookup failed for application {ApplicationId}", application.Id);
                    throw new AppException("BVN verification failed. Please ensure your BVN is correct.", 400);
                }
                
                // Deduct BVN lookup fee from company wallet (Mono BVN service fee, not OTP fee)
                try
                {
                    var settings = await _settingsService.GetSettingsAsync();
                    // Use OTP fee amount as BVN lookup fee (can be configured separately in future)
                    await _walletService.TransferFundsAsync(
                        (await _walletService.GetWalletByCompanyIdAsync(application.CompanyId))?.Id ?? Guid.Empty,
                        (await _walletService.GetSuperAdminWalletAsync())?.Id ?? Guid.Empty,
                        settings.OtpFee,
                        $"Mono BVN Lookup - Application {application.Id}",
                        null
                    );
                    _logger.LogInformation("BVN lookup fee deducted for application {ApplicationId}", application.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deduct BVN lookup fee for application {ApplicationId}, Company {CompanyId}",
                        application.Id, application.CompanyId);
                    // Don't throw - fee deduction failure shouldn't block BVN verification at this point
                }
                
                // Mark BVN as verified since Mono validated it
                application.BvnVerifiedAt = DateTime.UtcNow;
                application.CurrentStep = BorrowerOnboardingStep.Step2B_BvnValidated;
                await _borrowerRepository.UpdateAsync(application);
                
                _logger.LogInformation("Mono BVN lookup successful for application {ApplicationId}. BVN automatically verified.", application.Id);
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Mono BVN lookup for application {ApplicationId}", application.Id);
                throw new AppException("Unable to verify BVN. Please try again later.", 500);
            }
        }
        else
        {
            // For Remita: Generate BVN OTP
            _logger.LogInformation("Using Remita BVN OTP verification for application {ApplicationId}", application.Id);
            await GenerateBvnOtpAsync(new GenerateBvnOtpRequestDto { Email = application.Email });
        }

        return new BorrowerStep2ResponseDto();
    }

    public async Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Must have completed Step 2 at minimum
        if (application.CurrentStep < BorrowerOnboardingStep.Step2_BvnSubmitted)
        {
            throw new AppException("Please complete bank and BVN submission first", 400);
        }

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step2B_BvnValidated)
        {
            _logger.LogInformation("Borrower going back to Step 2B for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep3Onwards(application);
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

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Must have completed Step 2B at minimum
        if (application.CurrentStep < BorrowerOnboardingStep.Step2B_BvnValidated)
        {
            throw new AppException("Please complete BVN verification first", 400);
        }

        // Validate BVN is not already attached to a different user
        if (!string.IsNullOrEmpty(application.BVN))
        {
            var existingBvnApplication = await _borrowerRepository.GetByBvnAsync(application.BVN);
            if (existingBvnApplication != null && existingBvnApplication.Id != application.Id)
            {
                throw new AppException("BVN already exists", 409);
            }
        }

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step3_DocumentsUploaded)
        {
            _logger.LogInformation("Borrower going back to Step 3 for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep4Onwards(application);
        }

        // Validate at least 2 images are provided
        if (request.ImageIds == null || request.ImageIds.Count < 2)
        {
            throw new AppException("Please upload at least 2 ID documents", 400);
        }

        // Validate that all image IDs are not null or empty
        if (request.ImageIds.Any(id => string.IsNullOrWhiteSpace(id)))
        {
            throw new AppException("Invalid document image IDs provided. All image IDs must be valid.", 400);
        }

        // Calculate loan eligibility (enhanced with salary history)
        var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId) ?? throw new AppException("Loan product not found", 404);

        decimal minLoanEligible;
        decimal maxLoanEligible;
        var minTenor = product.MinTenor;
        var maxTenor = product.MaxTenor;

        // Ensure minTenor and maxTenor are valid (not 0)
        if (minTenor <= 0 || maxTenor <= 0)
        {
            _logger.LogWarning("Product {ProductId} has invalid tenor values (Min: {MinTenor}, Max: {MaxTenor}). Please update the product configuration.",
                product.Id, minTenor, maxTenor);
            throw new AppException("Loan product configuration error: Invalid tenor range. Please contact support.", 500);
        }

        // Require salary history check - borrower cannot proceed without it
        if (string.IsNullOrEmpty(application.BVN) || string.IsNullOrEmpty(application.AccountNo) || string.IsNullOrEmpty(application.BankCode))
        {
            throw new AppException("BVN and complete bank details are required to proceed. Please complete Step 2 properly.", 400);
        }

        // Fetch salary history from Remita - this is mandatory
        _logger.LogInformation("Starting salary history retrieval for application {ApplicationId} - Account:{Account}, Bank:{Bank}, BVN:{BVN}",
            application.Id, application.AccountNo, application.BankCode, application.BVN);

        var salaryHistoryResponse = await _remitaService.GetBorrowerSalaryHistoryAsync(
            application.AccountNo,
            application.BankCode,
            application.BVN);

        if (salaryHistoryResponse == null)
        {
            _logger.LogWarning("Salary history response was null for application {ApplicationId}", application.Id);
            throw new AppException("Unable to retrieve salary history from Remita. Please ensure your bank details are correct and you have salary payment history.", 400);
        }

        _logger.LogDebug("Salary history raw response for application {ApplicationId}: {Response}", application.Id, JsonSerializer.Serialize(salaryHistoryResponse));

        if (salaryHistoryResponse.Status != "success" || !salaryHistoryResponse.HasData)
        {
            _logger.LogWarning("Salary history returned no data/failed for application {ApplicationId} - Status:{Status} HasData:{HasData}",
                application.Id, salaryHistoryResponse.Status, salaryHistoryResponse.HasData);
            throw new AppException("Unable to retrieve salary history from Remita. Please ensure your bank details are correct and you have salary payment history.", 400);
        }

        // Save salary history to database

        await _salaryEligibilityService.SaveSalaryHistoryAsync(application.Id, salaryHistoryResponse);


        // Calculate eligibility based on salary history
        SalaryEligibilityDto eligibilityResult;
        eligibilityResult = await _salaryEligibilityService.CalculateLoanEligibilityAsync(salaryHistoryResponse, product);

        // Use calculated eligibility amounts
        minLoanEligible = eligibilityResult.FinalMinEligible;
        maxLoanEligible = eligibilityResult.FinalMaxEligible;

        // Log the eligibility calculation for debugging
        _logger.LogInformation("Salary-based eligibility calculated for application {ApplicationId}: Min={MinEligible}, Max={MaxEligible}, Reason={Reason}",
            application.Id, minLoanEligible, maxLoanEligible, eligibilityResult.EligibilityReason);

        // Check if borrower qualifies: max eligible amount must be at least the product's min amount
        if (maxLoanEligible < product.MinAmount)
        {
            _logger.LogWarning("Borrower does not qualify for loan product {ProductId}. Max eligible ({MaxEligible}) is less than product min ({ProductMin})",
                product.Id, maxLoanEligible, product.MinAmount);
            throw new AppException($"Unfortunately, you do not qualify for this loan product. Your maximum eligible amount ({maxLoanEligible:C}) is below the minimum loan amount ({product.MinAmount:C}) for this product.", 400);
        }

        // Adjust max eligible amount: use the smaller of calculated max or product max
        var finalMaxEligible = Math.Min(maxLoanEligible, product.MaxAmount);

        // Min eligible should always reflect the product's min amount
        var finalMinEligible = product.MinAmount;

        _logger.LogInformation("Final eligibility for application {ApplicationId}: Min={FinalMin} (Product Min), Max={FinalMax} (Lesser of Calculated {CalculatedMax} or Product Max {ProductMax})",
            application.Id, finalMinEligible, finalMaxEligible, maxLoanEligible, product.MaxAmount);

        // Update application
        application.Address = request.Address;
        application.IdNumber = request.IdNumber;
        application.DocumentIds = string.Join(",", request.ImageIds);
        application.MinLoanEligible = finalMinEligible;
        application.MaxLoanEligible = finalMaxEligible;
        application.MinTenor = minTenor;
        application.MaxTenor = maxTenor;
        application.CurrentStep = BorrowerOnboardingStep.Step3_DocumentsUploaded;
        application.DocumentsUploadedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        return new BorrowerStep3ResponseDto
        {
            MinLoanEligible = finalMinEligible,
            MaxLoanEligible = finalMaxEligible,
            MinTenor = minTenor,
            MaxTenor = maxTenor
        };
    }

    public async Task<BorrowerStep4ResponseDto> Step4_SubmitLoanApplicationAsync(BorrowerStep4RequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Must have completed Step 3 at minimum
        if (application.CurrentStep < BorrowerOnboardingStep.Step3_DocumentsUploaded)
        {
            throw new AppException("Please complete document upload first", 400);
        }

        // If already submitted, don't allow resubmission
        if (application.IsCompleted)
        {
            throw new AppException("Loan application has already been submitted", 400);
        }

        // Validate loan amount and tenor are within eligible range
        if (request.LoanAmount < application.MinLoanEligible || request.LoanAmount > application.MaxLoanEligible)
        {
            throw new AppException($"Loan amount must be between {application.MinLoanEligible:C} and {application.MaxLoanEligible:C}", 400);
        }

        if (request.Tenor <= 0)
        {
            throw new AppException("Tenor must be at least 1 month", 400);
        }

        if (request.Tenor < application.MinTenor || request.Tenor > application.MaxTenor)
        {
            throw new AppException($"Tenor must be between {application.MinTenor} and {application.MaxTenor} months", 400);
        }

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // EARLY VALIDATION: Check company wallet balance before proceeding with email notification
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            await _walletService.ValidateCompanyBalanceForFeeAsync(application.CompanyId, settings.EmailFee, "Loan Application Summary Email");
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            _logger.LogWarning(
                "AUDIT: Loan submission blocked for Company {CompanyName} (ID: {CompanyId}). Reason: {Reason}",
                companyName, application.CompanyId, ex.Message);
            throw new AppException(
                "We're unable to process your application at this time. Please contact support.",
                503);
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

        // Send loan application summary email to borrower
        var borrowerFullName = $"{application.FirstName} {application.LastName}";
        var emailSent = await _emailService.SendLoanApplicationSummaryEmailAsync(
            application.Email,
            borrowerFullName,
            request.LoanAmount,
            request.Tenor,
            monthlyRepayment,
            totalRepayment,
            product.Name,
            application.Company?.Name ?? "DevPay"
        );

        if (!emailSent)
        {
            _logger.LogWarning("Failed to send loan application summary email to {Email} for application {ApplicationId}",
                application.Email, application.Id);
            // Don't throw - email failure shouldn't block loan submission
        }
        else
        {
            // Deduct email fee from company wallet only after successful email send
            try
            {
                await DeductEmailFeeAsync(application.CompanyId, "Loan Application Summary");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deduct email fee for application {ApplicationId}, Company {CompanyId}",
                    application.Id, application.CompanyId);
                // Don't throw - fee deduction failure shouldn't block loan submission at this point
            }
        }

        return new BorrowerStep4ResponseDto
        {
            RepaymentAmount = totalRepayment,
            Tenor = request.Tenor,
            MonthlyRepaymentAmount = monthlyRepayment
        };
    }

    public async Task<UpdateDocumentsResponseDto> UpdateDocumentsAsync(UpdateDocumentsRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Validate at least 2 images are provided
        if (request.ImageIds == null || request.ImageIds.Count < 2)
        {
            throw new AppException("Please upload at least 2 ID documents", 400);
        }

        // Validate that all image IDs are not null or empty
        if (request.ImageIds.Any(id => string.IsNullOrWhiteSpace(id)))
        {
            throw new AppException("Invalid document image IDs provided. All image IDs must be valid.", 400);
        }

        // Update document IDs
        application.DocumentIds = string.Join(",", request.ImageIds);
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        _logger.LogInformation("Documents updated for application {ApplicationId}", application.Id);

        return new UpdateDocumentsResponseDto();
    }

    public async Task<RequestImageReuploadResponseDto> RequestImageReuploadAsync(RequestImageReuploadDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        if (string.IsNullOrWhiteSpace(application.Email))
        {
            throw new AppException("Borrower email not found", 400);
        }

        // Get client URL from configuration
        var clientUrl = _configuration["ClientUrl"] ?? "http://localhost:3000";
        var reuploadUrl = $"{clientUrl}/{application.Id}/readd-images";

        // Send email notification
        var borrowerName = $"{application.FirstName} {application.LastName}";
        var emailSent = await _emailService.SendImageReuploadRequestAsync(
            application.Email,
            borrowerName,
            request.Reason,
            reuploadUrl
        );

        if (!emailSent)
        {
            throw new AppException("Failed to send re-upload request email", 500);
        }

        _logger.LogInformation("Image re-upload request sent to borrower {BorrowerEmail} for application {ApplicationId}. Reason: {Reason}",
            application.Email, application.Id, request.Reason);

        return new RequestImageReuploadResponseDto();
    }

    public async Task<GenerateEmailOtpResponseDto> GenerateEmailOtpAsync(GenerateEmailOtpRequestDto request)
    {
        // Find application by email to get company information
        var application = await _borrowerRepository.GetByEmailAsync(request.EmailAddress) ?? throw new AppException("No application found for this email", 404);

        // Generate OTP
        var otp = GenerateOtp();

        // Update application with new OTP
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

        // Deduct OTP fee from company wallet only after successful email send
        await DeductOtpFeeAsync(application.CompanyId, "Email OTP");

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
        _logger.LogInformation("Attempting to generate BVN OTP for borrower with email: {Email}", request.Email);

        // Find application by email
        var application = await _borrowerRepository.GetByEmailAsync(request.Email);
        if (application == null)
        {
            _logger.LogWarning("No application found for email: {Email}", request.Email);
            throw new AppException("No application found with this email address.", 404);
        }

        // Ensure BVN was saved in Step 2
        if (string.IsNullOrEmpty(application.BVN))
        {
            _logger.LogWarning("Application {ApplicationId} for email {Email} has no BVN saved. Step 2 not completed.",
                application.Id, request.Email);
            throw new AppException("Please complete Step 2 (Submit Bank & BVN Info) first before generating BVN OTP.", 400);
        }

        // Generate BVN OTP
        var otp = GenerateOtp();

        // Update application with new OTP
        application.LastBvnOtp = otp;
        application.BvnOtpGeneratedAt = DateTime.UtcNow;
        await _borrowerRepository.UpdateAsync(application);

        // Send BVN OTP via email (temporary - should be SMS via BVN verification service in production)
        var emailSent = await _emailService.SendOtpEmailAsync(
            application.Email,
            otp,
            "BVN Verification"
        );

        if (!emailSent)
        {
            throw new AppException("Failed to send BVN verification code. Please try again.", 500);
        }

        // Deduct OTP fee from company wallet only after successful email send
        await DeductOtpFeeAsync(application.CompanyId, "BVN OTP");

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

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // EARLY VALIDATION: Check company wallet balance before resending OTP
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            await _walletService.ValidateCompanyBalanceForFeeAsync(application.CompanyId, settings.OtpFee, "Email OTP Resend");
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            _logger.LogWarning(
                "AUDIT: Borrower OTP resend blocked for Company {CompanyName} (ID: {CompanyId}). Reason: {Reason}",
                companyName, application.CompanyId, ex.Message);
            throw new AppException(
                "We're unable to process your request at this time. Please contact support.",
                503);
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

        // Deduct OTP fee from company wallet only after successful email send
        await DeductOtpFeeAsync(application.CompanyId, "Email OTP Resend");

        return new ResendStep1EmailOtpResponseDto();
    }

    /// <summary>
    /// Deducts OTP fee from company wallet and transfers to SuperAdmin wallet
    /// </summary>
    /// <param name="companyId">Company ID from the borrower application</param>
    /// <param name="otpType">Type of OTP being generated (for transaction description)</param>
    /// <returns>True if fee deduction was successful</returns>
    private async Task<bool> DeductOtpFeeAsync(Guid companyId, string otpType = "OTP")
    {
        try
        {
            // Get settings to determine OTP fee
            var settings = await _settingsService.GetSettingsAsync();
            var otpFee = settings.OtpFee;

            // Skip if OTP fee is 0 or negative
            if (otpFee <= 0)
            {
                return true; // No fee to deduct
            }

            // Get or create company wallet
            var companyWallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (companyWallet == null)
            {
                // Automatically create a wallet for the company
                companyWallet = await _walletService.CreateCompanyWalletAsync(companyId);
            }

            // Check if company has sufficient balance
            var hasSufficientBalance = await _walletService.HasSufficientBalanceAsync(companyWallet.Id, otpFee);
            if (!hasSufficientBalance)
            {
                throw new AppException($"Insufficient wallet balance to generate {otpType}. Please fund your wallet.", 400);
            }

            // Get or create SuperAdmin wallet
            var superAdminWallet = await _walletService.GetSuperAdminWalletAsync();
            if (superAdminWallet == null)
            {
                // Create SuperAdmin wallet if it doesn't exist
                superAdminWallet = await _walletService.CreateSuperAdminWalletAsync();
            }

            // Transfer funds from company wallet to SuperAdmin wallet
            var transferDescription = $"{otpType} generation fee - Company: {companyWallet.CompanyName ?? "Unknown"}";
            var transferSuccess = await _walletService.TransferFundsAsync(
                companyWallet.Id,
                superAdminWallet.Id,
                otpFee,
                transferDescription,
                null // System-initiated transfer - null for system transactions
            );

            if (!transferSuccess)
            {
                throw new AppException("Failed to process OTP fee. Please try again.", 500);
            }

            return true;
        }
        catch (AppException)
        {
            // Re-throw application exceptions (like insufficient balance)
            throw;
        }
        catch (Exception)
        {
            // Log the exception and throw a generic error
            throw new AppException("An error occurred while processing OTP fee. Please try again.", 500);
        }
    }

    /// <summary>
    /// Deducts Email fee from company wallet and transfers to SuperAdmin wallet
    /// </summary>
    /// <param name="companyId">Company ID from the borrower application</param>
    /// <param name="emailType">Type of email being sent (for transaction description)</param>
    /// <returns>True if fee deduction was successful</returns>
    private async Task<bool> DeductEmailFeeAsync(Guid companyId, string emailType = "Email")
    {
        try
        {
            // Get settings to determine Email fee
            var settings = await _settingsService.GetSettingsAsync();
            var emailFee = settings.EmailFee;

            // Skip if Email fee is 0 or negative
            if (emailFee <= 0)
            {
                return true; // No fee to deduct
            }

            // Get or create company wallet
            var companyWallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            // Automatically create a wallet for the company
            companyWallet ??= await _walletService.CreateCompanyWalletAsync(companyId);

            // Check if company has sufficient balance
            var hasSufficientBalance = await _walletService.HasSufficientBalanceAsync(companyWallet.Id, emailFee);
            if (!hasSufficientBalance)
            {
                throw new AppException($"Insufficient wallet balance to send {emailType}. Please fund your wallet.", 400);
            }

            // Get or create SuperAdmin wallet
            var superAdminWallet = await _walletService.GetSuperAdminWalletAsync();
            // Create SuperAdmin wallet if it doesn't exist
            superAdminWallet ??= await _walletService.CreateSuperAdminWalletAsync();

            // Transfer funds from company wallet to SuperAdmin wallet
            var transferDescription = $"{emailType} fee - Company: {companyWallet.CompanyName ?? "Unknown"}";
            var transferSuccess = await _walletService.TransferFundsAsync(
                companyWallet.Id,
                superAdminWallet.Id,
                emailFee,
                transferDescription,
                null // System-initiated transfer - null for system transactions
            );

            if (!transferSuccess)
            {
                throw new AppException("Failed to process Email fee. Please try again.", 500);
            }

            return true;
        }
        catch (AppException)
        {
            // Re-throw application exceptions (like insufficient balance)
            throw;
        }
        catch (Exception)
        {
            // Log the exception and throw a generic error
            throw new AppException("An error occurred while processing Email fee. Please try again.", 500);
        }
    }

    private static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
        // return "564312";
    }

    private static bool ValidateOtp(string identifier, string providedOtp, string? storedOtp, DateTime? generatedAt)
    {
        if (string.IsNullOrEmpty(storedOtp) || !generatedAt.HasValue)
        {
            return false;
        }

        // OTP expires after 10 minutes
        if (DateTime.UtcNow.Subtract(generatedAt.Value).TotalMinutes > 3)
        {
            return false;
        }

        return providedOtp == storedOtp;
    }

    public async Task<BorrowerCurrentStepResponseDto> GetCurrentStepAsync(BorrowerCurrentStepRequestDto request)
    {
        var borrowerApplication = await _borrowerRepository.GetByEmailAsync(request.Email);
        if (borrowerApplication == null)
        {
            // Check if it's a GUID to provide more specific error message
            if (Guid.TryParse(request.Email, out _))
            {
                throw new AppException($"No borrower application found for ID: {request.Email}", 404);
            }
            else
            {
                throw new AppException($"No borrower application found for email: {request.Email}", 404);
            }
        }

        var response = new BorrowerCurrentStepResponseDto
        {
            LoanId = borrowerApplication.Id,
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

    #region Step Data Clearing Methods

    /// <summary>
    /// Clears all data from Step 2 onwards (bank/BVN info, documents, loan eligibility, loan submission)
    /// </summary>
    private static void ClearStepsFromStep2Onwards(BorrowerApplication application)
    {
        // Clear Step 2 data
        application.BankCode = null;
        application.AccountNo = null;
        application.BVN = null;
        application.BvnVerifiedAt = null;
        application.LastBvnOtp = null;
        application.BvnOtpGeneratedAt = null;

        // Clear Step 3 and beyond
        ClearStepsFromStep3Onwards(application);
    }

    /// <summary>
    /// Clears all data from Step 2B onwards (BVN verification, documents, loan eligibility, loan submission)
    /// </summary>
    private static void ClearStepsFromStep2BOnwards(BorrowerApplication application)
    {
        // Clear Step 2B data
        application.BvnVerifiedAt = null;

        // Clear Step 3 and beyond
        ClearStepsFromStep3Onwards(application);
    }

    /// <summary>
    /// Clears all data from Step 3 onwards (documents, address, loan eligibility, loan submission)
    /// </summary>
    private static void ClearStepsFromStep3Onwards(BorrowerApplication application)
    {
        // Clear Step 3 data
        application.Address = null;
        application.IdNumber = null;
        application.DocumentIds = null;
        application.DocumentsUploadedAt = null;
        application.MinLoanEligible = null;
        application.MaxLoanEligible = null;
        application.MinTenor = null;
        application.MaxTenor = null;

        // Clear Step 4 and beyond
        ClearStepsFromStep4Onwards(application);
    }

    /// <summary>
    /// Clears all data from Step 4 onwards (loan submission)
    /// </summary>
    private static void ClearStepsFromStep4Onwards(BorrowerApplication application)
    {
        // Clear Step 4 data
        application.LoanId = null;
        application.LoanSubmittedAt = null;
        application.IsCompleted = false;
    }

    #endregion
}

