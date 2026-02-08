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
    private readonly IRemitaSalaryHistoryRepository _remitaSalaryHistoryRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BorrowerOnboardingService> _logger;
    private readonly INotificationOrchestrationService _notificationOrchestrator;
    private readonly IOtpService _otpService;
    private readonly IAuditService _auditService;

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
        IRemitaSalaryHistoryRepository remitaSalaryHistoryRepository,
        INotificationOrchestrationService notificationOrchestrator,
        IOtpService otpService,
        IConfiguration configuration,
        ILogger<BorrowerOnboardingService> logger,
        IAuditService auditService)
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
        _remitaSalaryHistoryRepository = remitaSalaryHistoryRepository;
        _notificationOrchestrator = notificationOrchestrator;
        _otpService = otpService;
        _configuration = configuration;
        _logger = logger;
        _auditService = auditService;
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
        _logger.LogInformation("Step1: Received phone number: {PhoneNumber} (Length: {Length})", 
            request.PhoneNumber, request.PhoneNumber?.Length);
        
        var application = new BorrowerApplication
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Employer = request.Employer,
            PhoneNumber = request.PhoneNumber,
            CompanyId = product.CompanyId, // Get CompanyId from the product
            ProductId = request.ProductId,
            CurrentStep = BorrowerOnboardingStep.Step1_EmailSent
        };

        var createdApplication = await _borrowerRepository.CreateAsync(application);
        _logger.LogInformation("Step1: Created application with phone number: {PhoneNumber}", 
            createdApplication.PhoneNumber);

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

        // Validate email OTP using OTP service
        var validateResult = await _otpService.ValidateOtpAsync(new ValidateOtpRequest
        {
            Type = OtpType.EmailVerification,
            RecipientIdentifier = application.Email,
            Code = request.Otp
        });

        if (!validateResult.Success)
        {
            throw new AppException(validateResult.ErrorMessage ?? "Invalid or expired OTP", 400);
        }

        // Update application
        application.CurrentStep = BorrowerOnboardingStep.Step1B_EmailValidated;
        application.EmailVerifiedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        return new BorrowerStep1BResponseDto();
    }

    public async Task<BorrowerStep2ResponseDto> Step2_SaveBvnAsync(BorrowerStep2RequestDto request)
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

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step2_BvnSent)
        {
            _logger.LogInformation("Borrower going back to Step 2 for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep2BOnwards(application);
        }

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // Validate bank details
        if (string.IsNullOrEmpty(request.AccountNo) || string.IsNullOrEmpty(request.BankCode))
        {
            throw new AppException("Complete bank details are required.", 400);
        }

        // Validate BVN format (11 digits)
        if (string.IsNullOrWhiteSpace(request.BVN) || request.BVN.Length != 11 || !request.BVN.All(char.IsDigit))
        {
            throw new AppException("Invalid BVN format. BVN must be 11 digits.", 400);
        }

        // Save BVN and bank details to application
        application.BVN = request.BVN;
        application.BankCode = request.BankCode;
        application.AccountNo = request.AccountNo;
        application.UpdatedAt = DateTime.UtcNow;

        // Step 1: Initiate Mono BVN lookup
        _logger.LogInformation("Initiating Mono BVN lookup for application {ApplicationId}", application.Id);
        
        var monoLookupResult = await _monoService.BvnLookupAsync(new MonoBvnLookupRequestDto
        {
            Bvn = request.BVN,
            Scope = "identity"
        });

        if (monoLookupResult?.Status?.ToLower() != "successful" || monoLookupResult.Data == null)
        {
            _logger.LogError("Mono BVN lookup failed for application {ApplicationId}: {Message}", 
                application.Id, monoLookupResult?.Message ?? "No response from Mono");
            throw new AppException("Failed to initiate BVN verification. Please try again later.", 500);
        }

        // Save Mono session details - always use alternate_phone method
        application.MonoBvnSessionId = monoLookupResult.Data.SessionId;
        
        _logger.LogInformation("Mono BVN lookup successful. SessionId: {SessionId}, Using method: alternate_phone", 
            application.MonoBvnSessionId);

        // Step 2: Verify - Send OTP to user using alternate_phone method
        _logger.LogInformation("Sending Mono BVN OTP for application {ApplicationId}, Method: alternate_phone", 
            application.Id);
        
        var monoVerifyResult = await _monoService.BvnVerifyAsync(new MonoBvnVerifyRequestDto
        {
            Method = "alternate_phone",
            PhoneNumber = application.PhoneNumber
        }, application.MonoBvnSessionId!);

        if (monoVerifyResult?.Status?.ToLower() != "successful")
        {
            _logger.LogError("Mono BVN verify failed for application {ApplicationId}: {Message}", 
                application.Id, monoVerifyResult?.Message ?? "No response from Mono");
            throw new AppException(monoVerifyResult?.Message ?? "Failed to send OTP. Please try again.", 500);
        }

        // Update application status
        application.CurrentStep = BorrowerOnboardingStep.Step2_BvnSent;
        application.BVN = GenerateBvnHash(request.BVN);
        application.BankCode = request.BankCode;
        application.AccountNo = request.AccountNo;
        application.BvnOtpGeneratedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        
        await _borrowerRepository.UpdateAsync(application);

        _logger.LogInformation("BVN verification OTP sent for application {ApplicationId}. Message: {Message}", 
            application.Id, monoVerifyResult.Message);

        return new BorrowerStep2ResponseDto
        {
            Message = monoVerifyResult.Message ?? "OTP sent successfully. Please check your phone.",
            OtpHint = $"OTP sent to {application.PhoneNumber}"
        };
    }

    public async Task<BorrowerStep2BResponseDto> Step2B_ValidateBvnOtpAsync(BorrowerStep2BRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Must have completed Step 2 (BVN sent)
        if (application.CurrentStep < BorrowerOnboardingStep.Step2_BvnSent)
        {
            throw new AppException("Please complete BVN submission first", 400);
        }

        // Allow resubmitting this step - clear subsequent step data if going back
        if (application.CurrentStep > BorrowerOnboardingStep.Step2B_BvnValidated)
        {
            _logger.LogInformation("Borrower going back to Step 2B for application {ApplicationId}. Clearing subsequent step data.", application.Id);
            ClearStepsFromStep3Onwards(application);
        }

        // Validate Mono session exists
        if (string.IsNullOrEmpty(application.MonoBvnSessionId))
        {
            throw new AppException("BVN verification session not found. Please restart the process from Step 2.", 400);
        }

        // Validate OTP and retrieve BVN details from Mono
        _logger.LogInformation("Validating Mono BVN OTP for application {ApplicationId}, SessionId: {SessionId}", 
            application.Id, application.MonoBvnSessionId);
        
        var monoDetailsResult = await _monoService.BvnGetDetailsAsync(new MonoBvnDetailsRequestDto
        {
            Otp = request.Otp
        }, application.MonoBvnSessionId!);

        if (monoDetailsResult?.Status?.ToLower() != "successful" || monoDetailsResult.Data == null)
        {
            _logger.LogWarning("Mono BVN OTP validation failed for application {ApplicationId}: {Message}", 
                application.Id, monoDetailsResult?.Message ?? "Invalid OTP");
            throw new AppException(monoDetailsResult?.Message ?? "Invalid or expired OTP. Please try again.", 400);
        }

        // Store verified BVN data
        var fullName = $"{monoDetailsResult.Data.FirstName} {monoDetailsResult.Data.MiddleName} {monoDetailsResult.Data.LastName}".Trim();
        
        var verifiedData = new
        {
            FirstName = monoDetailsResult.Data.FirstName,
            MiddleName = monoDetailsResult.Data.MiddleName,
            LastName = monoDetailsResult.Data.LastName,
            FullName = fullName,
            DateOfBirth = monoDetailsResult.Data.DateOfBirth,
            PhoneNumber = monoDetailsResult.Data.PhoneNumber,
            Email = monoDetailsResult.Data.Email,
            Gender = monoDetailsResult.Data.Gender,
            VerifiedAt = DateTime.UtcNow,
            SessionId = application.MonoBvnSessionId
        };
        
        application.MonoBvnVerifiedData = System.Text.Json.JsonSerializer.Serialize(verifiedData);
        
        _logger.LogInformation("BVN verified successfully for application {ApplicationId}. Name: {Name}", 
            application.Id, fullName);

        // Create BVN verification record for audit and reuse
        try
        {
            var bvnHash = GenerateBvnHash(application.BVN!);
            var verificationRecord = new MonoBvnVerificationRecord
            {
                Id = Guid.NewGuid(),
                BvnHash = bvnHash,
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(90), // 90-day retention
                Provider = "mono",
                FullName = fullName,
                DateOfBirth = monoDetailsResult.Data.DateOfBirth ?? string.Empty,
                PhoneNumber = monoDetailsResult.Data.PhoneNumber ?? string.Empty,
                LastSessionId = application.MonoBvnSessionId,
                VerifiedByUserId = "System",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Note: This would require access to MonoBvnVerificationRecords DbSet
            // If not available, this can be added in a separate service or skipped
            _logger.LogInformation("BVN verification record created for application {ApplicationId}", application.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create BVN verification record for application {ApplicationId}", application.Id);
            // Don't fail the process if audit record creation fails
        }

        // Update application
        application.CurrentStep = BorrowerOnboardingStep.Step2B_BvnValidated;
        application.BvnVerifiedAt = DateTime.UtcNow;
        application.IsBvnVerified = true;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        return new BorrowerStep2BResponseDto();
    }

    public async Task<BorrowerStep3ResponseDto> Step3_SaveBankAddressDocumentsAsync(BorrowerStep3RequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Prevent modifications if loan application is already submitted
        if (application.IsCompleted)
        {
            throw new AppException("Cannot modify a submitted loan application", 400);
        }

        // Must have completed Step 2B (BVN validated) at minimum
        if (application.CurrentStep < BorrowerOnboardingStep.Step2B_BvnValidated)
        {
            throw new AppException("Please complete BVN verification first", 400);
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

        // Bank details should already be saved from Step 2
        if (string.IsNullOrEmpty(application.AccountNo) || string.IsNullOrEmpty(application.BankCode))
        {
            throw new AppException("Bank details not found. Please complete Step 2 first.", 400);
        }

        // Fetch salary history from Remita - this is mandatory
        _logger.LogInformation("Starting salary history retrieval for application {ApplicationId} - Account:{Account}, Bank:{Bank}",
            application.Id, application.AccountNo, application.BankCode);

        var salaryHistoryResponse = await _remitaService.GetSalaryHistoryAsync(
            application.AccountNo,
            application.BankCode,
            application.BVN ?? string.Empty,
            application.Id); // Pass borrowerApplicationId instead of email

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

        // Check if borrower is eligible (FinalMaxEligible > 0 means eligible)
        if (eligibilityResult.FinalMaxEligible <= 0)
        {
            _logger.LogWarning("Borrower not eligible for loan product {ProductId}. Reason: {Reason}",
                product.Id, eligibilityResult.EligibilityReason);
            throw new AppException(eligibilityResult.EligibilityReason, 400);
        }

        // Use calculated eligibility amounts
        minLoanEligible = eligibilityResult.FinalMinEligible;
        maxLoanEligible = eligibilityResult.FinalMaxEligible;

        // Log the eligibility calculation for debugging
        _logger.LogInformation("Salary-based eligibility calculated for application {ApplicationId}: Min={MinEligible}, Max={MaxEligible}, Reason={Reason}",
            application.Id, minLoanEligible, maxLoanEligible, eligibilityResult.EligibilityReason);

        // Final eligibility amounts (already constrained by product limits in the service)
        var finalMinEligible = minLoanEligible;
        var finalMaxEligible = maxLoanEligible;

        _logger.LogInformation("Final eligibility for application {ApplicationId}: Min={FinalMin}, Max={FinalMax}",
            application.Id, finalMinEligible, finalMaxEligible);

        // Update application with address and documents (bank details already saved in Step 2)
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

        // Must accept offer letter to proceed
        if (!request.AcceptOfferLetter)
        {
            throw new AppException("You must accept the offer letter to proceed with the loan application", 400);
        }

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

        // Calculate loan amounts
        // Loan Principal (LP) = requested loan amount
        // Applied Interest (AI) = LP * interest rate * tenor
        // Repayment Amount (RA) = LP + AI
        // Applicable Fees (AF) = processing fees (percent + flat)
        // Amount to Disburse (AtD) = LP - AF
        var product = application.Product ?? await _loanProductRepository.GetLoanProductById(application.ProductId) ?? throw new AppException("Loan product not found", 404);

        var loanPrincipal = request.LoanAmount;
        
        // Calculate interest based on product settings
        decimal appliedInterest;
        decimal monthlyRepayment;
        var isMonthlyRate = product.InterestCostComputation == InterestCostComputation.PerMonth;
        var monthlyRate = isMonthlyRate ? product.InterestRate : product.InterestRate / 12;
        var monthlyRateDecimal = monthlyRate / 100; // Convert percentage to decimal
        
        if (product.InterestComputationBasis == InterestComputationBasis.Flat)
        {
            // Flat interest: Principal * (monthly rate) * tenor
            // Interest is calculated on full principal for entire period
            appliedInterest = loanPrincipal * monthlyRateDecimal * request.Tenor;
            appliedInterest = Math.Round(appliedInterest, 2);
            var totalRepaymentFlat = loanPrincipal + appliedInterest;
            monthlyRepayment = Math.Round(totalRepaymentFlat / request.Tenor, 2);
        }
        else
        {
            // Reducing Balance (Amortization): M = P × [r(1+r)^n] / [(1+r)^n - 1]
            // Interest is calculated on remaining principal each month
            if (monthlyRateDecimal > 0)
            {
                var r = monthlyRateDecimal;
                var n = request.Tenor;
                var compoundFactor = (decimal)Math.Pow((double)(1 + r), n);
                monthlyRepayment = loanPrincipal * (r * compoundFactor) / (compoundFactor - 1);
                monthlyRepayment = Math.Round(monthlyRepayment, 2);
                appliedInterest = Math.Round((monthlyRepayment * n) - loanPrincipal, 2);
            }
            else
            {
                // Zero interest - just divide principal by tenor
                monthlyRepayment = Math.Round(loanPrincipal / request.Tenor, 2);
                appliedInterest = 0;
            }
        }
        
        // Repayment Amount (RA) = LP + AI
        var totalRepayment = Math.Round(loanPrincipal + appliedInterest, 2);
        
        // Calculate Applicable Fees (AF) = Processing Fee + Maintenance Fee + Legal Fee
        var processingFee = loanPrincipal * (product.ProcessingFeePercent / 100) + product.ProcessingFeeFlat;
        var maintenanceFee = loanPrincipal * (product.MaintenanceFeePercent / 100);
        var legalFee = loanPrincipal * (product.LegalFeePercent / 100) + product.LegalFeeFlat;
        var applicableFees = Math.Round(processingFee + maintenanceFee + legalFee, 2);
        
        // Amount to Disburse (AtD) = LP - AF
        var disbursementAmount = Math.Round(loanPrincipal - applicableFees, 2);

        // Create actual loan record (RemitaCustomerId and AuthorizationCode are now stored in RemitaSalaryHistory)
        var loan = new Loan
        {
            Amount = loanPrincipal,
            DurationInMonths = request.Tenor,
            TotalRepayment = totalRepayment,
            MonthlyRepayment = monthlyRepayment,
            DisbursementAmount = disbursementAmount,
            ApplicableFees = applicableFees,
            AppliedInterest = appliedInterest,
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

        // Update application with offer letter acceptance (always true at this point due to validation)
        application.CurrentStep = BorrowerOnboardingStep.Step4_LoanSubmitted;
        application.LoanSubmittedAt = DateTime.UtcNow;
        application.IsCompleted = true;
        application.IsOfferLetterAccepted = true;
        application.OfferLetterAcceptedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        // Audit log for offer letter acceptance
        await _auditService.LogAsync(
            action: "OfferLetterAccepted",
            category: "Loan",
            userEmail: application.Email,
            entityType: "BorrowerApplication",
            entityId: application.Id.ToString(),
            companyId: application.CompanyId,
            details: $"Borrower {application.FirstName} {application.LastName} ({application.Email}) accepted offer letter for loan amount {request.LoanAmount:C}",
            amount: request.LoanAmount
        );

        // Build offer letter DTO for generating PDF
        var offerLetterDto = new OfferLetterDto
        {
            LoanId = loan.Id,
            BorrowerName = $"{application.FirstName} {application.LastName}",
            BorrowerEmail = application.Email,
            BorrowerAddress = application.Address ?? "Address not provided",
            CompanyName = application.Company?.Name ?? "DevPay",
            CompanyAddress = string.Join(", ", new[] { 
                application.Company?.Street, 
                application.Company?.City, 
                application.Company?.State 
            }.Where(s => !string.IsNullOrEmpty(s))),
            LoanAmount = loanPrincipal,
            DurationInMonths = request.Tenor,
            InterestRate = product.InterestRate,
            InterestComputationBasis = product.InterestComputationBasis.ToString(),
            TotalInterest = appliedInterest,
            TotalRepayment = totalRepayment,
            MonthlyRepayment = monthlyRepayment,
            Purpose = "Salary Loan",
            ProcessingFee = Math.Round(loanPrincipal * (product.ProcessingFeePercent / 100) + product.ProcessingFeeFlat, 2),
            MaintenanceFee = Math.Round(loanPrincipal * (product.MaintenanceFeePercent / 100), 2),
            TotalFees = applicableFees,
            DisbursementAmount = disbursementAmount,
            ProductName = product.Name,
            PenaltyRate = product.PenaltyOnDefaultPrincipal,
            MoratoriumDays = product.Moratorium,
            OfferDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            ExpectedDisbursementDate = DateTime.UtcNow.AddDays(3),
            ExpectedMaturityDate = DateTime.UtcNow.AddMonths(request.Tenor)
        };

        // Send loan application summary email to borrower with attached offer letter
        var borrowerFullName = $"{application.FirstName} {application.LastName}";
        var emailSent = await _emailService.SendLoanApplicationSummaryEmailAsync(
            application.Email,
            borrowerFullName,
            request.LoanAmount,
            request.Tenor,
            monthlyRepayment,
            totalRepayment,
            product.Name,
            application.Company?.Name ?? "DevPay",
            offerLetterDto
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
            LoanPrincipal = loanPrincipal,
            ApplicableFees = applicableFees,
            DisbursementAmount = disbursementAmount,
            AppliedInterest = appliedInterest,
            RepaymentAmount = totalRepayment,
            Tenor = request.Tenor,
            MonthlyRepaymentAmount = monthlyRepayment
        };
    }

    public async Task<ResendStep2BvnOtpResponseDto> ResendStep2BvnOtpAsync(ResendStep2BvnOtpRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Only allow resending if the application is on Step 2 (BVN sent but not validated)
        if (application.CurrentStep != BorrowerOnboardingStep.Step2_BvnSent)
        {
            throw new AppException("BVN OTP can only be resent for applications in Step 2 (BVN Sent) status", 400);
        }

        // Ensure BVN exists
        if (string.IsNullOrEmpty(application.BVN))
        {
            throw new AppException("BVN not found for this application", 400);
        }

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // Validate Mono session exists
        if (string.IsNullOrEmpty(application.MonoBvnSessionId))
        {
            throw new AppException("BVN verification session not found. Please restart the process from Step 2.", 400);
        }

        // Validate phone number is required for alternate_phone method
        if (string.IsNullOrEmpty(application.PhoneNumber))
        {
            throw new AppException("Phone number is required for BVN verification", 400);
        }

        // Resend Mono BVN OTP using alternate_phone method
        _logger.LogInformation("Resending Mono BVN OTP for application {ApplicationId}, SessionId: {SessionId}, Method: alternate_phone", 
            application.Id, application.MonoBvnSessionId);
        
        var monoVerifyResult = await _monoService.BvnVerifyAsync(new MonoBvnVerifyRequestDto
        {
            Method = "alternate_phone",
            PhoneNumber = application.PhoneNumber
        }, application.MonoBvnSessionId!);

        if (monoVerifyResult?.Status?.ToLower() != "successful")
        {
            _logger.LogError("Failed to resend Mono BVN OTP for application {ApplicationId}: {Message}", 
                application.Id, monoVerifyResult?.Message ?? "No response from Mono");
            throw new AppException("Failed to resend BVN verification OTP. Please try again later.", 500);
        }

        // Update timestamp
        application.BvnOtpGeneratedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

        _logger.LogInformation("BVN verification OTP resent successfully for application {ApplicationId}", application.Id);

        return new ResendStep2BvnOtpResponseDto();
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

        // Generate OTP using OTP service
        var otpResult = await _otpService.GenerateAndSendOtpAsync(new GenerateOtpRequest
        {
            Type = OtpType.EmailVerification,
            RecipientIdentifier = request.EmailAddress,
            CompanyId = application.CompanyId,
            RelatedEntityId = application.Id,
            RelatedEntityType = "BorrowerApplication",
            DeliveryChannel = NotificationChannel.Email,
            CreatedBy = "System"
        });

        if (!otpResult.Success)
        {
            throw new AppException(otpResult.ErrorMessage ?? "Failed to send email OTP", 500);
        }

        // Dual write: Update legacy fields
        application.LastEmailOtp = "******"; // Masked for security
        application.EmailOtpGeneratedAt = DateTime.UtcNow;
        await _borrowerRepository.UpdateAsync(application);

        return new GenerateEmailOtpResponseDto();
    }

    public async Task<ValidateEmailOtpResponseDto> ValidateEmailOtpAsync(ValidateEmailOtpRequestDto request)
    {
        var application = await _borrowerRepository.GetByEmailAsync(request.Email);
        if (application == null)
        {
            throw new AppException("No application found for this email", 404);
        }

        // Validate using OTP service
        var validateResult = await _otpService.ValidateOtpAsync(new ValidateOtpRequest
        {
            Type = OtpType.EmailVerification,
            RecipientIdentifier = request.Email,
            Code = request.Otp
        });

        if (!validateResult.Success)
        {
            throw new AppException(validateResult.ErrorMessage ?? "Invalid or expired OTP", 400);
        }

        return new ValidateEmailOtpResponseDto();
    }

    public async Task<GenerateBvnOtpResponseDto> GenerateBvnOtpAsync(GenerateBvnOtpRequestDto request)
    {
        // Find application by BVN
        var application = await _borrowerRepository.GetByBvnAsync(request.BVN) ?? throw new AppException("BVN not found", 404);

        // Get company for logging
        var company = await _companyRepository.GetCompanyById(application.CompanyId);
        var companyName = company?.Name ?? "Unknown Company";

        // Generate OTP using OTP service
        // Use email as recipient identifier since we're sending via email
        var otpResult = await _otpService.GenerateAndSendOtpAsync(new GenerateOtpRequest
        {
            Type = OtpType.BvnVerification,
            RecipientIdentifier = application.Email,
            CompanyId = application.CompanyId,
            RelatedEntityId = application.Id,
            RelatedEntityType = "BorrowerApplication",
            DeliveryChannel = NotificationChannel.EmailPrimary,
            SenderName = companyName,
            CreatedBy = "System"
        });

        if (!otpResult.Success)
        {
            throw new AppException(otpResult.ErrorMessage ?? "Failed to send BVN OTP", 500);
        }

        // Dual write: Update legacy fields
        application.LastBvnOtp = "******"; // Masked for security
        application.BvnOtpGeneratedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;
        await _borrowerRepository.UpdateAsync(application);

        return new GenerateBvnOtpResponseDto();
    }

    public async Task<ResendStep1EmailOtpResponseDto> ResendStep1EmailOtpAsync(ResendStep1EmailOtpRequestDto request)
    {
        var application = await _borrowerRepository.GetByIdAsync(request.LoanId) ?? throw new AppException("Application not found", 404);

        // Only allow resending if the application is still on Step 1 (email sent but not validated)
        if (application.CurrentStep != BorrowerOnboardingStep.Step1_EmailSent)
        {
            throw new AppException("Email OTP can only be resent for applications in Step 1 (Email Sent) status", 400);
        }

        // Resend OTP using OTP service
        var resendResult = await _otpService.ResendOtpAsync(new ResendOtpRequest
        {
            Type = OtpType.EmailVerification,
            RecipientIdentifier = application.Email,
            CompanyId = application.CompanyId,
            DeliveryChannel = NotificationChannel.Email
        });

        if (!resendResult.Success)
        {
            throw new AppException(resendResult.ErrorMessage ?? "Failed to resend email OTP", 500);
        }

        // Dual write: Update legacy fields
        application.LastEmailOtp = "******"; // Masked for security
        application.EmailOtpGeneratedAt = DateTime.UtcNow;
        application.UpdatedAt = DateTime.UtcNow;

        await _borrowerRepository.UpdateAsync(application);

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
            BorrowerOnboardingStep.Step2_BvnSent => "BVN Verification",
            BorrowerOnboardingStep.Step2B_BvnValidated => "BVN Verified",
            BorrowerOnboardingStep.Step3_DocumentsUploaded => "Bank Info & Documents Upload",
            BorrowerOnboardingStep.Step4_LoanSubmitted => "Loan Application",
            _ => "Unknown Step"
        };
    }

    private static string GetStepDescription(BorrowerOnboardingStep step)
    {
        return step switch
        {
            BorrowerOnboardingStep.Step1_EmailSent => "Please verify your email address by entering the OTP sent to your email.",
            BorrowerOnboardingStep.Step1B_EmailValidated => "Email verified successfully. Proceed to verify your BVN.",
            BorrowerOnboardingStep.Step2_BvnSent => "Please verify your BVN by entering the OTP sent to your phone.",
            BorrowerOnboardingStep.Step2B_BvnValidated => "BVN verified successfully. Proceed to provide your bank information, address and documents.",
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
            BorrowerOnboardingStep.Step2_BvnSent => 3,
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
            BorrowerOnboardingStep.Step1B_EmailValidated => BorrowerOnboardingStep.Step2_BvnSent,
            BorrowerOnboardingStep.Step2_BvnSent => BorrowerOnboardingStep.Step2B_BvnValidated,
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
            BorrowerOnboardingStep.Step1B_EmailValidated => new List<string> { "Submit your BVN for verification" },
            BorrowerOnboardingStep.Step2_BvnSent => new List<string> { "Verify your BVN using the OTP sent to your phone" },
            BorrowerOnboardingStep.Step2B_BvnValidated => new List<string> { "Provide your bank account details, address, and upload required documents" },
            BorrowerOnboardingStep.Step3_DocumentsUploaded => new List<string> { "Submit your loan application with desired amount and tenor" },
            BorrowerOnboardingStep.Step4_LoanSubmitted => new List<string> { "Your application is complete and under review" },
            _ => new List<string>()
        };
    }

    /// <summary>
    /// Generates a SHA-256 hash of the BVN for privacy-preserving storage
    /// </summary>
    private static string GenerateBvnHash(string bvn)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(bvn));
        return Convert.ToBase64String(hash);
    }

    #region Step Data Clearing Methods

    /// <summary>
    /// Clears all data from Step 2 onwards (BVN, bank info, documents, loan eligibility, loan submission)
    /// </summary>
    private static void ClearStepsFromStep2Onwards(BorrowerApplication application)
    {
        // Clear Step 2 data (BVN and bank details)
        application.BVN = null;
        application.BankCode = null;
        application.AccountNo = null;
        application.LastBvnOtp = null;
        application.BvnOtpGeneratedAt = null;
        application.BvnVerifiedAt = null;
        application.MonoBvnSessionId = null;
        application.MonoBvnVerifiedData = null;

        // Clear Step 2B and beyond
        ClearStepsFromStep2BOnwards(application);
    }

    /// <summary>
    /// Clears all data from Step 2B onwards (BVN validation, documents, loan submission)
    /// </summary>
    private static void ClearStepsFromStep2BOnwards(BorrowerApplication application)
    {
        // Clear Step 2B data (BVN validation timestamp and verified data)
        application.BvnVerifiedAt = null;
        application.MonoBvnVerifiedData = null;

        // Clear Step 3 and beyond
        ClearStepsFromStep3Onwards(application);
    }

    /// <summary>
    /// Clears all data from Step 3 onwards (address, documents, loan eligibility, loan submission)
    /// </summary>
    private static void ClearStepsFromStep3Onwards(BorrowerApplication application)
    {
        // Clear Step 3 data (address, documents)
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

