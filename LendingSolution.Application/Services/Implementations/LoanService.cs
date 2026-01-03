using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Dtos.Response;
using Microsoft.Extensions.Configuration;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class LoanService(
    UserManager<ApplicationUser> userManager,
    IRemitaService remitaService,
    ILoanRepository loanRepository,
    ICompanyRepository companyRepository,
    IBorrowerApplicationRepository borrowerApplicationRepository,
    IConfiguration configuration,
    IEmailService emailService,
    IDocumentService documentService,
    IProvidusDisbursementService providusDisbursementService
) : ILoanService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IBorrowerApplicationRepository _borrowerApplicationRepository = borrowerApplicationRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IDocumentService _documentService = documentService;
    private readonly IProvidusDisbursementService _providusDisbursementService = providusDisbursementService;

    public async Task<String> Register(RegisterRequestDto body)
    {
        var existingCompany = await _companyRepository.GetCompanyById(body.CompanyId);
        if (existingCompany is null)
        {
            throw new AppException("Company not found", 404);
        }

        var existingEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == body.Email);

        if (existingEmail != null)
        {
            throw new AppException("A user with this email already exists");
        }

        var existingBvn = await _borrowerApplicationRepository.GetByBvnAsync(body.Bvn);

        if (existingBvn != null)
        {
            throw new AppException("A user with this BVN already exists");
        }

        var user = new ApplicationUser
        {
            UserName = body.Email,
            Email = body.Email,
            FirstName = string.Empty,
            LastName = string.Empty,
            Address = string.Empty,
            City = string.Empty,
            State = string.Empty,
            DateOfBirth = body.DateOfBirth,
            PhoneNumber = body.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            throw new AppException(
                "User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // add a new loan
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = 0,
            DurationInMonths = 0,
            Purpose = string.Empty,
            Status = LoanStatus.NotBooked,
            CompanyId = body.CompanyId
        };

        var loanResult = await _loanRepository.CreateLoan(loan);

        if (loanResult is false)
        {
            throw new AppException("Loan creation failed");
        }

        return loan.Id.ToString();
    }

    public async Task<Loan> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            throw new AppException("User not found", 404);
        }

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = dto.Amount,
            DurationInMonths = dto.DurationInMonths,
            Purpose = dto.Purpose,
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _loanRepository.CreateLoan(loan);
        if (!created)
        {
            throw new AppException("Failed to create loan application", 400);
        }

        // Return the loan from the repository to get the saved entity
        var savedLoan = await _loanRepository.GetLoanById(loan.Id);
        if (savedLoan == null)
        {
            throw new AppException("Failed to retrieve created loan", 500);
        }

        return savedLoan;
    }

    public async Task<Loan> ApproveLoan(Guid loanId, string? approvedBy = null, string? reason = null)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);

        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        if (loan.Status == LoanStatus.Approved || loan.Status == LoanStatus.OfferLetterSent || loan.Status == LoanStatus.OfferLetterSigned)
        {
            throw new AppException("Loan already approved", 400);
        }

        if (loan.Status == LoanStatus.Rejected)
        {
            throw new AppException("Cannot approve a rejected loan", 400);
        }

        if (loan.Status == LoanStatus.Disbursed)
        {
            throw new AppException("Loan has already been disbursed", 400);
        }

        // Set loan to Approved status first
        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.ApprovedBy = approvedBy;
        loan.ProcessingReason = reason;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan status", 500);
        }

        // Send offer letter after approval
        try
        {
            await SendOfferLetterAsync(loanId, approvedBy);
        }
        catch (Exception)
        {
            // Log but don't fail the approval if offer letter sending fails
            // The admin can resend the offer letter manually
        }

        return loan;
    }

    public async Task<Loan> RejectLoan(Guid loanId, string? rejectedBy = null, string? reason = null)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);

        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        if (loan.Status == LoanStatus.Approved)
        {
            throw new AppException("Cannot reject an approved loan", 400);
        }

        if (loan.Status == LoanStatus.Rejected)
        {
            throw new AppException("Loan already rejected", 400);
        }

        if (loan.Status == LoanStatus.Disbursed)
        {
            throw new AppException("Cannot reject a disbursed loan", 400);
        }

        loan.Status = LoanStatus.Rejected;
        loan.RejectedAt = DateTime.UtcNow;
        loan.RejectedBy = rejectedBy;
        loan.ProcessingReason = reason;

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan status", 500);
        }

        return loan;
    }

    public async Task<List<Loan>> GetPendingLoans()
    {
        return await _loanRepository.GetPendingLoans();
    }

    public async Task<List<Loan>> GetLoansByStatus(LoanStatus status)
    {
        return await _loanRepository.GetLoansByStatus(status);
    }

    public async Task<Loan> ProcessLoan(Guid loanId, ProcessLoanRequestDto request, string processedBy)
    {
        return request.Action.ToLower() switch
        {
            "approve" => await ApproveLoan(loanId, processedBy, request.Reason),
            "reject" => await RejectLoan(loanId, processedBy, request.Reason),
            _ => throw new AppException("Invalid action. Use 'approve' or 'reject'", 400)
        };
    }

    public async Task<List<Loan>> GetAllLoans()
    {
        return await _loanRepository.GetAllLoansWithIncludes();
    }

    public async Task<PagedLoanListDto> GetAllLoansAsync(LoanFilterDto filter)
    {
        var query = _loanRepository.GetAllLoansQueryable();

        // Apply filters
        query = ApplyFilters(query, filter);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate summary statistics
        var totalAmount = await query.SumAsync(l => l.Amount);
        var avgAmount = totalCount > 0 ? totalAmount / totalCount : 0;
        var statusCounts = await query
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var pagedLoans = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Map to DTOs
        var loanDtos = pagedLoans.Select(MapToLoanListDto).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanListDto
        {
            Loans = loanDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1,
            TotalLoanAmount = totalAmount,
            AverageAmount = avgAmount,
            StatusCounts = statusCounts
        };
    }

    public async Task<PagedLoanListDto> GetCompanyLoansAsync(Guid companyId, LoanFilterDto filter)
    {
        var query = _loanRepository.GetCompanyLoansQueryable(companyId);

        // Apply filters (excluding company filter since it's already filtered)
        query = ApplyFilters(query, filter, excludeCompanyFilter: true);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate summary statistics
        var totalAmount = await query.SumAsync(l => l.Amount);
        var avgAmount = totalCount > 0 ? totalAmount / totalCount : 0;
        var statusCounts = await query
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var pagedLoans = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Map to DTOs
        var loanDtos = pagedLoans.Select(MapToLoanListDto).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanListDto
        {
            Loans = loanDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1,
            TotalLoanAmount = totalAmount,
            AverageAmount = avgAmount,
            StatusCounts = statusCounts
        };
    }

    public async Task<LoanListDto> GetLoanByIdAsync(Guid loanId, string? requestingUserId = null)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);

        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        // Optional: Add access control if requestingUserId is provided
        // This can be enhanced based on business rules

        return MapToLoanListDto(loan);
    }

    private IQueryable<Loan> ApplyFilters(IQueryable<Loan> query, LoanFilterDto filter, bool excludeCompanyFilter = false)
    {
        // Search filter (user name, email, purpose)
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(l =>
                (l.User != null && (
                    l.User.FirstName.ToLower().Contains(searchTerm) ||
                    l.User.LastName.ToLower().Contains(searchTerm) ||
                    (l.User.Email != null && l.User.Email.ToLower().Contains(searchTerm))
                )) ||
                (l.BorrowerApplication != null && (
                    l.BorrowerApplication.FirstName.ToLower().Contains(searchTerm) ||
                    l.BorrowerApplication.LastName.ToLower().Contains(searchTerm) ||
                    l.BorrowerApplication.Email.ToLower().Contains(searchTerm)
                )) ||
                l.Purpose.ToLower().Contains(searchTerm));
        }

        // Company filter (only for SuperAdmin view)
        if (!excludeCompanyFilter && filter.CompanyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == filter.CompanyId.Value);
        }

        // Status filter
        if (filter.Status.HasValue)
        {
            query = query.Where(l => l.Status == filter.Status.Value);
        }

        // Amount range filter
        if (filter.MinAmount.HasValue)
        {
            query = query.Where(l => l.Amount >= filter.MinAmount.Value);
        }
        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(l => l.Amount <= filter.MaxAmount.Value);
        }

        // Duration range filter
        if (filter.MinDuration.HasValue)
        {
            query = query.Where(l => l.DurationInMonths >= filter.MinDuration.Value);
        }
        if (filter.MaxDuration.HasValue)
        {
            query = query.Where(l => l.DurationInMonths <= filter.MaxDuration.Value);
        }

        // Date range filters
        if (filter.StartDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= filter.StartDate.Value);
        }
        if (filter.EndDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= filter.EndDate.Value);
        }

        // Approval date filters
        if (filter.ApprovedAfter.HasValue)
        {
            query = query.Where(l => l.ApprovedAt >= filter.ApprovedAfter.Value);
        }
        if (filter.ApprovedBefore.HasValue)
        {
            query = query.Where(l => l.ApprovedAt <= filter.ApprovedBefore.Value);
        }

        // Product filter
        if (filter.ProductId.HasValue)
        {
            query = query.Where(l => l.ProductId == filter.ProductId.Value);
        }

        // Mandate filter
        if (filter.IsMandateGenerated.HasValue)
        {
            query = query.Where(l => l.IsMandateGenerated == filter.IsMandateGenerated.Value);
        }

        return query;
    }

    private IQueryable<Loan> ApplySorting(IQueryable<Loan> query, LoanFilterDto filter)
    {
        return filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Amount)
                : query.OrderBy(l => l.Amount),
            "duration" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.DurationInMonths)
                : query.OrderBy(l => l.DurationInMonths),
            "status" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Status)
                : query.OrderBy(l => l.Status),
            "username" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.User != null ? l.User.FirstName : l.BorrowerApplication != null ? l.BorrowerApplication.FirstName : string.Empty)
                       .ThenByDescending(l => l.User != null ? l.User.LastName : l.BorrowerApplication != null ? l.BorrowerApplication.LastName : string.Empty)
                : query.OrderBy(l => l.User != null ? l.User.FirstName : l.BorrowerApplication != null ? l.BorrowerApplication.FirstName : string.Empty)
                       .ThenBy(l => l.User != null ? l.User.LastName : l.BorrowerApplication != null ? l.BorrowerApplication.LastName : string.Empty),
            "companyname" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Company.Name)
                : query.OrderBy(l => l.Company.Name),
            "approvedat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.ApprovedAt)
                : query.OrderBy(l => l.ApprovedAt),
            "duedate" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.DueDate)
                : query.OrderBy(l => l.DueDate),
            "updatedat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.UpdatedAt)
                : query.OrderBy(l => l.UpdatedAt),
            "createdat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.CreatedAt)
                : query.OrderBy(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };
    }

    private LoanListDto MapToLoanListDto(Loan loan)
    {
        // Get user information from User or BorrowerApplication
        string firstName;
        string lastName;
        string email;

        if (loan.User != null)
        {
            firstName = loan.User.FirstName;
            lastName = loan.User.LastName;
            email = loan.User.Email ?? string.Empty;
        }
        else if (loan.BorrowerApplication != null)
        {
            firstName = loan.BorrowerApplication.FirstName;
            lastName = loan.BorrowerApplication.LastName;
            email = loan.BorrowerApplication.Email;
        }
        else
        {
            // Fallback - this should rarely happen
            firstName = "Unknown";
            lastName = "User";
            email = string.Empty;
        }

        return new LoanListDto
        {
            Id = loan.Id,
            UserId = loan.UserId ?? string.Empty,
            UserFirstName = firstName,
            UserLastName = lastName,
            UserEmail = email,
            Amount = loan.Amount,
            DurationInMonths = loan.DurationInMonths,
            Purpose = loan.Purpose,
            Status = loan.Status,
            ApprovedAt = loan.ApprovedAt,
            DueDate = loan.DueDate,
            RejectedAt = loan.RejectedAt,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt,
            CompanyId = loan.CompanyId,
            CompanyName = loan.Company.Name,
            CompanyShortName = loan.Company.ShortName,
            ProductId = loan.ProductId,
            ProductName = loan.Product.Name,
            ProductInterestRate = loan.Product.InterestRate,
            Message = loan.Message,
            IsMandateGenerated = loan.IsMandateGenerated,
            MandateId = loan.MandateId,
            DocumentIds = loan.BorrowerApplication?.DocumentIds,
            OfferLetterDocumentId = loan.OfferLetterDocumentId,
            OfferLetterUrl = loan.OfferLetterUrl,
            SignedOfferLetterDocumentId = loan.SignedOfferLetterDocumentId,
            SalaryHistory = MapToSalaryHistoryDto(loan.BorrowerApplication?.RemitaSalaryHistory)
        };
    }

    private SalaryHistoryInfoDto? MapToSalaryHistoryDto(RemitaSalaryHistory? salaryHistory)
    {
        if (salaryHistory == null)
            return null;

        return new SalaryHistoryInfoDto
        {
            CompanyName = salaryHistory.CompanyName ?? string.Empty,
            CustomerName = salaryHistory.CustomerName ?? string.Empty,
            Category = salaryHistory.Category ?? string.Empty,
            SalaryCount = salaryHistory.SalaryCount,
            AverageMonthlySalary = salaryHistory.AverageMonthlySalary,
            LatestSalaryAmount = salaryHistory.LatestSalaryAmount,
            LatestPaymentDate = salaryHistory.LatestPaymentDate,
            MinSalaryAmount = salaryHistory.MinSalaryAmount,
            MaxSalaryAmount = salaryHistory.MaxSalaryAmount,
            ConsistentMonths = salaryHistory.ConsistentMonths,
            HasOutstandingLoans = salaryHistory.HasOutstandingLoans,
            TotalOutstandingAmount = salaryHistory.TotalOutstandingAmount,
            FirstPaymentDate = salaryHistory.FirstPaymentDate
        };
    }

    #region Offer Letter and Disbursement Methods

    public async Task<OfferLetterDto> GetOfferLetterDetailsAsync(Guid loanId)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        return BuildOfferLetterDto(loan);
    }

    public async Task<OfferLetterResponseDto> SendOfferLetterAsync(Guid loanId, string? approvedBy = null)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        // Only allow sending offer letter for Approved status or resending for OfferLetterSent
        if (loan.Status != LoanStatus.Approved && loan.Status != LoanStatus.OfferLetterSent)
        {
            throw new AppException($"Cannot send offer letter for loan with status: {loan.Status}", 400);
        }

        // Build offer letter details
        var offerLetterDto = BuildOfferLetterDto(loan);

        // Send the offer letter email
        var emailSent = await _emailService.SendOfferLetterEmailAsync(offerLetterDto);

        if (!emailSent)
        {
            throw new AppException("Failed to send offer letter email", 500);
        }

        // Update loan status to OfferLetterSent
        loan.Status = LoanStatus.OfferLetterSent;
        loan.OfferLetterSentAt = DateTime.UtcNow;

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan status", 500);
        }

        return new OfferLetterResponseDto
        {
            LoanId = loanId,
            Status = LoanStatus.OfferLetterSent.ToString(),
            Message = "Offer letter sent successfully. Please check your email.",
            OfferLetterUrl = loan.OfferLetterUrl,
            SentAt = loan.OfferLetterSentAt,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Offer valid for 7 days
        };
    }

    public async Task<SignedOfferLetterResponseDto> UploadSignedOfferLetterAsync(Guid loanId, SignedOfferLetterUploadDto dto, string uploadedBy)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        // Only allow upload for OfferLetterSent status
        if (loan.Status != LoanStatus.OfferLetterSent && loan.Status != LoanStatus.OfferLetterSigned)
        {
            throw new AppException($"Cannot upload signed offer letter for loan with status: {loan.Status}. Offer letter must be sent first.", 400);
        }

        // Validate document exists and has completed upload
        var document = await _documentService.GetDocumentByIdAsync(dto.SignedOfferLetterDocumentId);
        if (document == null)
        {
            throw new AppException("Signed offer letter document not found. Please upload the document first.", 404);
        }
        if (document.Status != Core.Enum.DocumentStatus.Completed)
        {
            throw new AppException($"Signed offer letter document upload is {document.Status}. Please wait for upload to complete or retry upload.", 400);
        }

        // Validate that document is a PDF
        var pdfExtension = ".pdf";
        var documentExtension = document.FileExtension?.ToLowerInvariant();
        if (string.IsNullOrEmpty(documentExtension) || documentExtension != pdfExtension)
        {
            throw new AppException("Signed offer letter must be a PDF file.", 400);
        }

        // Update loan with signed offer letter document ID
        loan.SignedOfferLetterDocumentId = Guid.Parse(dto.SignedOfferLetterDocumentId);
        loan.SignedOfferLetterUploadedAt = DateTime.UtcNow;
        loan.Status = LoanStatus.OfferLetterSigned;

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan with signed offer letter", 500);
        }

        return new SignedOfferLetterResponseDto
        {
            LoanId = loanId,
            Status = LoanStatus.OfferLetterSigned.ToString(),
            Message = "Signed offer letter uploaded successfully. Loan is now ready for disbursement.",
            SignedOfferLetterDocumentId = loan.SignedOfferLetterDocumentId?.ToString(),
            UploadedAt = loan.SignedOfferLetterUploadedAt,
            ReadyForDisbursement = true
        };
    }

    public async Task<LoanDisbursementResponseDto> DisburseLoanAsync(Guid loanId, string? disbursedBy = null)
    {
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        // Only allow disbursement for OfferLetterSigned status
        if (loan.Status != LoanStatus.OfferLetterSigned)
        {
            throw new AppException($"Cannot disburse loan with status: {loan.Status}. Signed offer letter must be uploaded first.", 400);
        }

        // Verify signed offer letter document exists
        if (!loan.SignedOfferLetterDocumentId.HasValue)
        {
            throw new AppException("Signed offer letter not found. Please upload the signed offer letter before disbursement.", 400);
        }

        // Get borrower details for transfer
        string borrowerEmail;
        string borrowerName;
        string? accountNumber = null;
        string? bankCode = null;

        if (loan.BorrowerApplication != null)
        {
            borrowerEmail = loan.BorrowerApplication.Email;
            borrowerName = $"{loan.BorrowerApplication.FirstName} {loan.BorrowerApplication.LastName}";
            accountNumber = loan.BorrowerApplication.AccountNo;
            bankCode = loan.BorrowerApplication.BankCode;
        }
        else if (loan.User != null)
        {
            borrowerEmail = loan.User.Email ?? string.Empty;
            borrowerName = $"{loan.User.FirstName} {loan.User.LastName}";
            // Note: ApplicationUser doesn't have account details - must be provided via loan or separate lookup
            // For now, account details must come from BorrowerApplication
        }
        else
        {
            throw new AppException("Borrower information not found", 400);
        }

        // Validate account details for disbursement
        if (string.IsNullOrEmpty(accountNumber))
        {
            throw new AppException("Borrower account number not found. Cannot process disbursement.", 400);
        }

        // Initiate fund transfer via Providus
        var disbursementRequest = new ProvidusDisbursementInternalRequestDto
        {
            LoanId = loanId,
            DestinationAccountNumber = accountNumber,
            DestinationBankCode = bankCode ?? string.Empty,
            Amount = loan.Amount,
            Narration = $"Loan Disbursement for {borrowerName} - Loan ID: {loanId.ToString()[..8]}",
            BeneficiaryName = borrowerName
        };

        var disbursementResult = await _providusDisbursementService.TransferFundsAsync(disbursementRequest);

        if (!disbursementResult.IsSuccessful)
        {
            throw new AppException($"Disbursement failed: {disbursementResult.Message}. {disbursementResult.ErrorDetails}", 500);
        }

        // Update loan to Disbursed status
        loan.Status = LoanStatus.Disbursed;
        loan.DisbursementDate = disbursementResult.DisbursedAt;
        loan.DisbursementReference = disbursementResult.DisbursementReference;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            // Log critical: money was transferred but loan status update failed
            throw new AppException("CRITICAL: Disbursement successful but failed to update loan status. Reference: " + disbursementResult.DisbursementReference, 500);
        }

        // Send disbursement notification email
        if (!string.IsNullOrEmpty(borrowerEmail))
        {
            await _emailService.SendDisbursementNotificationAsync(borrowerEmail, borrowerName, loan.Amount, disbursementResult.DisbursementReference);
        }

        return new LoanDisbursementResponseDto
        {
            LoanId = loanId,
            Status = LoanStatus.Disbursed.ToString(),
            Message = disbursementResult.IsMockTransaction
                ? "Loan has been disbursed successfully (MOCK MODE)."
                : "Loan has been disbursed successfully.",
            Amount = loan.Amount,
            DisbursedAt = loan.DisbursementDate,
            DisbursementReference = disbursementResult.DisbursementReference,
            DueDate = loan.DueDate
        };
    }

    private OfferLetterDto BuildOfferLetterDto(Loan loan)
    {
        // Get borrower details
        string borrowerName;
        string borrowerEmail;
        string borrowerAddress;

        if (loan.BorrowerApplication != null)
        {
            borrowerName = $"{loan.BorrowerApplication.FirstName} {loan.BorrowerApplication.LastName}";
            borrowerEmail = loan.BorrowerApplication.Email;
            borrowerAddress = loan.BorrowerApplication.Address ?? "Address not provided";
        }
        else if (loan.User != null)
        {
            borrowerName = $"{loan.User.FirstName} {loan.User.LastName}";
            borrowerEmail = loan.User.Email ?? string.Empty;
            borrowerAddress = loan.User.Address ?? "Address not provided";
        }
        else
        {
            throw new AppException("Borrower information not found", 400);
        }

        // Calculate loan details
        var interestRate = loan.Product.InterestRate;
        var isMonthlyRate = loan.Product.InterestCostComputation == InterestCostComputation.PerMonth;
        var monthlyRate = isMonthlyRate ? interestRate : interestRate / 12;

        decimal totalInterest;
        if (loan.Product.InterestComputationBasis == InterestComputationBasis.Flat)
        {
            totalInterest = loan.Amount * (monthlyRate / 100) * loan.DurationInMonths;
        }
        else // Reducing balance
        {
            // Simple approximation for reducing balance
            totalInterest = loan.Amount * (monthlyRate / 100) * loan.DurationInMonths * 0.55m;
        }

        var totalRepayment = loan.Amount + totalInterest;
        var monthlyRepayment = totalRepayment / loan.DurationInMonths;

        // Company address
        var companyAddress = string.Join(", ", new[]
        {
            loan.Company.Street,
            loan.Company.City,
            loan.Company.State
        }.Where(s => !string.IsNullOrEmpty(s)));

        return new OfferLetterDto
        {
            LoanId = loan.Id,
            BorrowerName = borrowerName,
            BorrowerEmail = borrowerEmail,
            BorrowerAddress = borrowerAddress,
            CompanyName = loan.Company.Name,
            CompanyAddress = string.IsNullOrEmpty(companyAddress) ? "Company Address" : companyAddress,
            LoanAmount = loan.Amount,
            DurationInMonths = loan.DurationInMonths,
            InterestRate = interestRate,
            InterestComputationBasis = loan.Product.InterestComputationBasis.ToString(),
            TotalInterest = Math.Round(totalInterest, 2),
            TotalRepayment = Math.Round(totalRepayment, 2),
            MonthlyRepayment = Math.Round(monthlyRepayment, 2),
            Purpose = loan.Purpose,
            ProductName = loan.Product.Name,
            PenaltyRate = loan.Product.PenaltyOnDefaultPrincipal,
            MoratoriumDays = loan.Product.Moratorium,
            OfferDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            ExpectedDisbursementDate = DateTime.UtcNow.AddDays(3),
            ExpectedMaturityDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths),
            OfferLetterUrl = loan.OfferLetterUrl
        };
    }

    #endregion
}
