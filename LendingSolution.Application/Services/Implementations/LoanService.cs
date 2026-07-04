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
using Microsoft.Extensions.Logging;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class LoanService(
    UserManager<ApplicationUser> userManager,
    IRemitaService remitaService,
    ILoanRepository loanRepository,
    ICompanyRepository companyRepository,
    IBorrowerApplicationRepository borrowerApplicationRepository,
    IRemitaSalaryHistoryRepository remitaSalaryHistoryRepository,
    IConfiguration configuration,
    IEmailService emailService,
    IDocumentService documentService,
    IProvidusDisbursementService providusDisbursementService,
    ApplicationDbContext db,
    ILogger<LoanService> logger,
    IAuditService auditService
) : ILoanService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IBorrowerApplicationRepository _borrowerApplicationRepository = borrowerApplicationRepository;
    private readonly IRemitaSalaryHistoryRepository _remitaSalaryHistoryRepository = remitaSalaryHistoryRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IDocumentService _documentService = documentService;
    private readonly IProvidusDisbursementService _providusDisbursementService = providusDisbursementService;
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<LoanService> _logger = logger;

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

        // BVN is persisted as a SHA-256 hash, so hash the incoming plaintext before lookup.
        var bvnHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(body.Bvn ?? string.Empty)));
        var existingBvn = await _borrowerApplicationRepository.GetByBvnAsync(bvnHash);

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
        loan.Reason = reason;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan status", 500);
        }

        // Audit log
        await auditService.LogAsync(
            action: "LoanApproved",
            category: "Loan",
            userId: approvedBy,
            entityType: "Loan",
            entityId: loanId.ToString(),
            companyId: loan.CompanyId,
            details: $"Loan {loan.Id} for {loan.User?.Email} approved. Amount: {loan.Amount:C}. Reason: {reason}",
            amount: loan.Amount
        );

        // Offer letter is now sent during borrower onboarding Step 4
        // No need to send it again during approval

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
        loan.Reason = reason;

        var updateResult = await _loanRepository.UpdateLoan(loan);
        if (!updateResult)
        {
            throw new AppException("Failed to update loan status", 500);
        }

        // Audit log
        await auditService.LogAsync(
            action: "LoanRejected",
            category: "Loan",
            userId: rejectedBy,
            entityType: "Loan",
            entityId: loanId.ToString(),
            companyId: loan.CompanyId,
            details: $"Loan {loan.Id} for {loan.User?.Email} rejected. Amount: {loan.Amount:C}. Reason: {reason}",
            amount: loan.Amount
        );

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
        if (filter.IsMandateCreated.HasValue)
        {
            query = query.Where(l => l.IsMandateCreated == filter.IsMandateCreated.Value);
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
            IsMandateCreated = loan.IsMandateCreated,
            MandateRef = loan.MandateRef,
            DocumentIds = loan.BorrowerApplication?.DocumentIds,
            OfferLetterDocumentId = loan.OfferLetterDocumentId,
            OfferLetterUrl = loan.OfferLetterUrl,
            SignedOfferLetterDocumentId = loan.SignedOfferLetterDocumentId,
            SalaryHistory = MapToSalaryHistoryDto(loan.BorrowerApplication?.RemitaSalaryHistory)
        };
    }

    private static SalaryHistoryInfoDto? MapToSalaryHistoryDto(RemitaSalaryHistory? salaryHistory)
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
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId) ?? throw new AppException("Loan not found", 404);

        // Only allow upload for OfferLetterSent status
        if (loan.Status != LoanStatus.OfferLetterSent && loan.Status != LoanStatus.OfferLetterSigned)
        {
            throw new AppException($"Cannot upload signed offer letter for loan with status: {loan.Status}. Offer letter must be sent first.", 400);
        }

        // Validate document exists and has completed upload
        var document = await _documentService.GetDocumentByIdAsync(dto.SignedOfferLetterDocumentId) ?? throw new AppException("Signed offer letter document not found. Please upload the document first.", 404);
        if (document.Status != Core.Enum.DocumentStatus.Completed)
        {
            throw new AppException($"Signed offer letter document upload is {document.Status}. Please wait for upload to complete or retry upload.", 400);
        }

        // Validate that document is a PDF
        var documentType = document.DocumentType?.ToLower() ?? string.Empty;
        if (string.IsNullOrEmpty(documentType) || documentType != "pdf")
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
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId) ?? throw new AppException("Loan not found", 404);

        // Allow disbursement for Approved or OfferLetterSigned status
        // Offer letter acceptance is now captured during borrower onboarding (Step 4)
        if (loan.Status != LoanStatus.Approved && loan.Status != LoanStatus.OfferLetterSigned)
        {
            throw new AppException($"Cannot disburse loan with status: {loan.Status}. Loan must be approved first.", 400);
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
        else
        {
            throw new AppException("Borrower information not found", 400);
        }

        // Validate account details for disbursement
        if (string.IsNullOrEmpty(accountNumber))
        {
            throw new AppException("Borrower account number not found. Cannot process disbursement.", 400);
        }

        if (string.IsNullOrEmpty(bankCode))
        {
            throw new AppException("Borrower bank code not found. Cannot process disbursement.", 400);
        }

        // Get repayment amounts from loan (already calculated and saved during loan application)
        if (!loan.TotalRepayment.HasValue || !loan.MonthlyRepayment.HasValue)
        {
            throw new AppException("Loan repayment amounts not found. Cannot process disbursement.", 400);
        }

        var totalRepayment = loan.TotalRepayment.Value;
        var monthlyRepayment = loan.MonthlyRepayment.Value;

        // Get phone number for mandate
        var phoneNumber = loan.BorrowerApplication?.PhoneNumber;
        if (string.IsNullOrEmpty(phoneNumber))
        {
            throw new AppException("Borrower phone number not found. Cannot create mandate.", 400);
        }

        // Get CustomerId and AuthorisationCode from RemitaSalaryHistory table
        if (loan.BorrowerApplication == null)
        {
            throw new AppException("Borrower application not found. Cannot retrieve salary history.", 400);
        }

        var salaryHistory = await _remitaSalaryHistoryRepository.GetByBorrowerApplicationIdAsync(loan.BorrowerApplication.Id);
        if (salaryHistory == null || string.IsNullOrEmpty(salaryHistory.CustomerId) || string.IsNullOrEmpty(salaryHistory.AuthorisationCode))
        {
            throw new AppException("Customer ID or authorization code not found in salary history. Cannot create mandate.", 400);
        }

        // Create Remita mandate before disbursement using the same authorization code from salary history
        // var customerId = salaryHistory.CustomerId;
        // var authorisationCode = salaryHistory.AuthorisationCode;
        // var disbursementDate = DateTime.UtcNow.ToString("dd-MM-yyyy HH:mm:ss") + "+0000";
        // var firstCollectionDate = DateTime.UtcNow.AddMonths(1).ToString("dd-MM-yyyy HH:mm:ss") + "+0000";

        // _logger.LogInformation("=== PREPARING TO CREATE REMITA MANDATE ===");
        
        // var mandateResult = await _remitaService.CreateMandateAsync(
        //     customerId: customerId,
        //     phoneNumber: phoneNumber,
        //     accountNumber: accountNumber,
        //     loanAmount: loan.Amount.ToString("F2"),
        //     collectionAmount: monthlyRepayment.ToString("F2"),
        //     dateOfDisbursement: disbursementDate,
        //     dateOfCollection: firstCollectionDate,
        //     totalCollectionAmount: totalRepayment.ToString("F2"),
        //     numberOfRepayments: loan.DurationInMonths.ToString(),
        //     bankCode: bankCode,
        //     authorisationCode
        // );

        // _logger.LogInformation("Mandate creation response for loan {LoanId}: {@MandateResult}", loanId, mandateResult);

        // if (mandateResult == null || mandateResult.ResponseCode != "00")
        // {
        //     var errorMessage = mandateResult?.ResponseMsg ?? "Failed to create Remita mandate";
        //     _logger.LogError("=== MANDATE CREATION FAILED ===");
        //     _logger.LogError("Loan ID: {LoanId}", loanId);
        //     _logger.LogError("Mandate Result is null: {IsNull}", mandateResult == null);
        //     _logger.LogError("Response Code: {ResponseCode}", mandateResult?.ResponseCode);
        //     _logger.LogError("Response Message: {ResponseMessage}", mandateResult?.ResponseMsg);
        //     _logger.LogError("Status: {Status}", mandateResult?.Status);
        //     _logger.LogError("Has Data: {HasData}", mandateResult?.HasData);
        //     _logger.LogError("Error Message: {Error}", errorMessage);
        //     throw new AppException($"Cannot disburse loan. Mandate creation failed: {errorMessage}", 400);
        // }

        // // Store mandate reference in loan
        // loan.MandateRef = mandateResult.Data?.MandateReference ?? string.Empty;
        // loan.IsMandateCreated = true;
        // loan.MandateCreatedAt = DateTime.UtcNow;
        // _logger.LogInformation("Mandate created successfully for loan {LoanId}. MandateReference: {MandateReference}",
        //     loanId, loan.MandateRef);

        // Get disbursement amount (Amount to Disburse = Principal - Applicable Fees)
        var disbursementAmount = loan.DisbursementAmount ?? loan.Amount; // Fallback to loan.Amount for legacy loans
        
        // Initiate fund transfer via Providus
        var disbursementRequest = new ProvidusDisbursementInternalRequestDto
        {
            LoanId = loanId,
            DestinationAccountNumber = accountNumber,
            DestinationBankCode = bankCode,
            Amount = disbursementAmount,
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

        // Create repayment record
        var repayment = new Repayment
        {
            LoanId = loanId,
            TotalDue = loan.TotalRepayment ?? totalRepayment,
            TotalRepaid = 0,
            AmountUnpaid = loan.TotalRepayment ?? totalRepayment,
            Status = RepaymentStatus.Active,
            LastPaymentAt = null
        };

        _db.Repayments.Add(repayment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created repayment record for loan {LoanId} with TotalDue: {TotalDue}",
            loanId, repayment.TotalDue);

        // Audit log for disbursement
        await auditService.LogAsync(
            action: "LoanDisbursed",
            category: "Loan",
            userId: disbursedBy,
            entityType: "Loan",
            entityId: loanId.ToString(),
            companyId: loan.CompanyId,
            details: $"Loan {loan.Id} disbursed to {borrowerName} ({borrowerEmail}). Reference: {disbursementResult.DisbursementReference}",
            amount: disbursementAmount
        );

        // Send disbursement notification email
        if (!string.IsNullOrEmpty(borrowerEmail))
        {
            await _emailService.SendDisbursementNotificationAsync(borrowerEmail, borrowerName, disbursementAmount, disbursementResult.DisbursementReference);
        }

        return new LoanDisbursementResponseDto
        {
            LoanId = loanId,
            Status = LoanStatus.Disbursed.ToString(),
            Message = disbursementResult.IsMockTransaction
                ? "Loan has been disbursed successfully (MOCK MODE)."
                : "Loan has been disbursed successfully.",
            Amount = disbursementAmount,
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
        var monthlyRateDecimal = monthlyRate / 100; // Convert percentage to decimal

        decimal totalInterest;
        decimal monthlyRepayment;
        
        if (loan.Product.InterestComputationBasis == InterestComputationBasis.Flat)
        {
            // Flat interest: Principal * (monthly rate) * tenor
            totalInterest = loan.Amount * monthlyRateDecimal * loan.DurationInMonths;
            var totalRepaymentFlat = loan.Amount + totalInterest;
            monthlyRepayment = totalRepaymentFlat / loan.DurationInMonths;
        }
        else // Reducing balance (Amortization)
        {
            // M = P × [r(1+r)^n] / [(1+r)^n - 1]
            if (monthlyRateDecimal > 0)
            {
                var r = monthlyRateDecimal;
                var n = loan.DurationInMonths;
                var compoundFactor = (decimal)Math.Pow((double)(1 + r), n);
                monthlyRepayment = loan.Amount * (r * compoundFactor) / (compoundFactor - 1);
                totalInterest = (monthlyRepayment * n) - loan.Amount;
            }
            else
            {
                monthlyRepayment = loan.Amount / loan.DurationInMonths;
                totalInterest = 0;
            }
        }

        var totalRepayment = loan.Amount + totalInterest;

        // Calculate fees
        var processingFee = loan.Amount * (loan.Product.ProcessingFeePercent / 100) + loan.Product.ProcessingFeeFlat;
        var maintenanceFee = loan.Amount * (loan.Product.MaintenanceFeePercent / 100);
        var totalFees = processingFee + maintenanceFee;
        var disbursementAmount = loan.Amount - totalFees;

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
            ProcessingFee = Math.Round(processingFee, 2),
            MaintenanceFee = Math.Round(maintenanceFee, 2),
            TotalFees = Math.Round(totalFees, 2),
            DisbursementAmount = Math.Round(disbursementAmount, 2),
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

    /// <summary>
    /// Stops Remita mandate collection for a loan
    /// </summary>
    public async Task<RemitaStopMandateResponseDto> StopLoanCollectionAsync(Guid loanId, string? stoppedBy = null)
    {
        _logger.LogInformation("Attempting to stop loan collection for loan {LoanId}", loanId);

        // Get the loan with necessary includes
        var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId);
        if (loan == null)
        {
            _logger.LogWarning("Loan not found: {LoanId}", loanId);
            throw new AppException("Loan not found", 404);
        }

        // Verify loan has a mandate
        if (string.IsNullOrEmpty(loan.MandateRef))
        {
            _logger.LogWarning("Loan {LoanId} does not have a mandate reference", loanId);
            throw new AppException("Loan does not have an active mandate", 400);
        }

        if (!loan.IsMandateCreated)
        {
            _logger.LogWarning("Loan {LoanId} mandate was not created", loanId);
            throw new AppException("Loan mandate was not successfully created", 400);
        }

        // Get borrower application to retrieve Remita customer details
        var borrowerApplication = await _borrowerApplicationRepository.GetByLoanIdAsync(loanId);
        if (borrowerApplication == null)
        {
            _logger.LogWarning("Borrower application not found for loan {LoanId}", loanId);
            throw new AppException("Borrower application not found", 404);
        }

        // Get RemitaSalaryHistory to retrieve CustomerId and AuthorisationCode
        var salaryHistory = await _remitaSalaryHistoryRepository.GetByBorrowerApplicationIdAsync(borrowerApplication.Id);
        if (salaryHistory == null || string.IsNullOrEmpty(salaryHistory.CustomerId) || string.IsNullOrEmpty(salaryHistory.AuthorisationCode))
        {
            _logger.LogWarning("Remita salary history or authorization code not found for loan {LoanId}", loanId);
            throw new AppException("Remita customer information not found. Cannot stop mandate.", 404);
        }

        // Call Remita to stop the mandate using the same authorization code from salary history
        var result = await _remitaService.StopMandateAsync(
            salaryHistory.CustomerId,
            loan.MandateRef,
            salaryHistory.AuthorisationCode
        );

        if (result == null || result.Status?.ToLower() == "fail")
        {
            _logger.LogError("Failed to stop mandate for loan {LoanId}. Remita response: {@Result}", loanId, result);
            throw new AppException("Failed to stop loan collection in Remita", 500);
        }

        // Update loan status to indicate mandate is stopped
        loan.IsMandateCreated = false;
        loan.MandateStoppedAt = DateTime.UtcNow;
        loan.MandateStoppedBy = stoppedBy;

        await _loanRepository.UpdateLoan(loan);

        _logger.LogInformation("Successfully stopped mandate for loan {LoanId}, MandateRef: {MandateRef}",
            loanId, loan.MandateRef);

        return result;
    }

    /// <summary>
    /// Processes loan collection notifications from Remita webhook
    /// </summary>
    /// <param name="notification">The notification DTO from Remita</param>
    /// <param name="payload">Optional raw JSON payload</param>
    /// <returns>The created RemitaLoanCollectionNotification entity</returns>
    public async Task<RemitaLoanCollectionNotification?> ProcessLoanCollectionNotificationAsync(RemitaLoanCollectionNotificationDto notification, string? payload = null)
    {
        try
        {
            _logger.LogInformation("Processing Remita loan collection notification - ID: {RemitaId}, MandateRef: {MandateRef}, Amount: {Amount}, Status: {Status}",
                notification.Id, notification.MandateRef, notification.Amount, notification.PaymentStatus);

            // Check if this notification already exists to avoid duplicates
            var existingNotification = await _db.RemitaLoanCollectionNotifications
                .FirstOrDefaultAsync(n => n.RemitaId == notification.Id);

            if (existingNotification != null)
            {
                _logger.LogWarning("Notification with RemitaId {RemitaId} already exists. Skipping duplicate.", notification.Id);
                return existingNotification;
            }

            // Parse payment date - Remita sends in format "24-08-2021 14:56:53+0000"
            DateTime? paymentDate = null;
            if (!string.IsNullOrEmpty(notification.PaymentDate))
            {
                try
                {
                    // Try parsing the date string - handle different formats
                    if (DateTime.TryParseExact(notification.PaymentDate, "dd-MM-yyyy HH:mm:sszzz",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                    {
                        paymentDate = parsedDate;
                    }
                    else if (DateTime.TryParse(notification.PaymentDate, out DateTime parsedDate2))
                    {
                        paymentDate = parsedDate2;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse payment date: {PaymentDate}", notification.PaymentDate);
                }
            }

            // Create new notification entity
            var loanCollectionNotification = new RemitaLoanCollectionNotification
            {
                RemitaId = notification.Id,
                Amount = notification.Amount,
                ModuleName = notification.ModuleName,
                NotificationSent = notification.NotificationSent,
                NetSalary = notification.NetSalary,
                TotalCredit = notification.TotalCredit,
                MandateRef = notification.MandateRef,
                BalanceDue = notification.BalanceDue,
                CustomerId = notification.CustomerId,
                PaymentDate = paymentDate,
                PaymentStatus = notification.PaymentStatus,
                RawPayload = JsonSerializer.Serialize(notification),
                Payload = payload ?? JsonSerializer.Serialize(notification)
            };

            _db.RemitaLoanCollectionNotifications.Add(loanCollectionNotification);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Successfully saved Remita loan collection notification with ID: {Id}, MandateRef: {MandateRef}",
                loanCollectionNotification.Id, loanCollectionNotification.MandateRef);

            // Update repayment record if payment was successful
            if (!string.IsNullOrEmpty(notification.PaymentStatus) &&
                notification.PaymentStatus.Equals("successful", StringComparison.OrdinalIgnoreCase) &&
                notification.Amount > 0)
            {
                await UpdateRepaymentFromNotificationAsync(notification.MandateRef, notification.Amount, paymentDate);
            }

            return loanCollectionNotification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing loan collection notification for RemitaId: {RemitaId}, MandateRef: {MandateRef}",
                notification.Id, notification.MandateRef);
            throw;
        }
    }

    /// <summary>
    /// Updates repayment record when a successful payment notification is received
    /// </summary>
    private async Task UpdateRepaymentFromNotificationAsync(string mandateRef, decimal amount, DateTime? paymentDate)
    {
        try
        {
            // Find loan by mandate reference
            var loan = await _db.Loans
                .Include(l => l.BorrowerApplication)
                .FirstOrDefaultAsync(l => l.MandateRef == mandateRef);

            if (loan == null)
            {
                _logger.LogWarning("Loan not found for MandateRef: {MandateRef}. Cannot update repayment.", mandateRef);
                return;
            }

            // Find repayment record for this loan
            var repayment = await _db.Repayments
                .FirstOrDefaultAsync(r => r.LoanId == loan.Id);

            if (repayment == null)
            {
                _logger.LogWarning("Repayment record not found for LoanId: {LoanId}. Cannot update repayment.", loan.Id);
                return;
            }

            // Update repayment totals
            repayment.TotalRepaid += amount;
            repayment.AmountUnpaid = repayment.TotalDue - repayment.TotalRepaid;
            repayment.LastPaymentAt = paymentDate ?? DateTime.UtcNow;

            // Update status based on repayment progress
            if (repayment.AmountUnpaid <= 0)
            {
                repayment.Status = RepaymentStatus.Completed;
                repayment.AmountUnpaid = 0; // Ensure no negative values

                // Update loan status to Repaid
                loan.Status = LoanStatus.Repaid;
                _db.Loans.Update(loan);

                _logger.LogInformation("Loan {LoanId} fully repaid. Total repaid: {TotalRepaid}", loan.Id, repayment.TotalRepaid);
            }
            else
            {
                // Check if overdue
                if (loan.DueDate.HasValue && DateTime.UtcNow > loan.DueDate.Value && repayment.AmountUnpaid > 0)
                {
                    repayment.Status = RepaymentStatus.Overdue;
                    loan.Status = LoanStatus.Overdue;
                    _db.Loans.Update(loan);
                }
                else
                {
                    repayment.Status = RepaymentStatus.Active;
                }
            }

            _db.Repayments.Update(repayment);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Updated repayment for LoanId: {LoanId}. Amount: {Amount}, TotalRepaid: {TotalRepaid}, AmountUnpaid: {AmountUnpaid}, Status: {Status}",
                loan.Id, amount, repayment.TotalRepaid, repayment.AmountUnpaid, repayment.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating repayment for MandateRef: {MandateRef}", mandateRef);
            // Don't throw - we don't want to fail the notification processing
        }
    }

    /// <summary>
    /// Hybrid Reconciliation: Fetches mandate history from Remita and reconciles with local records
    /// Combines webhook notifications with API polling for comprehensive reconciliation
    /// </summary>
    /// <param name="loanId">The loan ID to reconcile</param>
    /// <param name="reconciledBy">User performing the reconciliation</param>
    /// <returns>Reconciliation result with details</returns>
    public async Task<LoanReconciliationResponseDto> ReconcileLoanCollectionAsync(Guid loanId, string? reconciledBy = null)
    {
        try
        {
            _logger.LogInformation("Starting hybrid reconciliation for loan {LoanId}", loanId);

            // Get loan with related data
            var loan = await _loanRepository.GetLoanByIdWithIncludes(loanId) ?? throw new AppException("Loan not found", 404);
            if (string.IsNullOrEmpty(loan.MandateRef))
            {
                throw new AppException("Loan does not have a mandate reference. Cannot reconcile.", 400);
            }

            if (!loan.IsMandateCreated)
            {
                throw new AppException("Mandate has not been created for this loan. Cannot reconcile.", 400);
            }

            // Get borrower application for validation
            var borrowerApplication = loan.BorrowerApplication ?? throw new AppException("Borrower application not found", 404);

            // Get RemitaSalaryHistory to retrieve CustomerId and AuthorisationCode
            var salaryHistory = await _remitaSalaryHistoryRepository.GetByBorrowerApplicationIdAsync(borrowerApplication.Id);
            if (salaryHistory == null || string.IsNullOrEmpty(salaryHistory.CustomerId) || string.IsNullOrEmpty(salaryHistory.AuthorisationCode))
            {
                _logger.LogWarning("Remita salary history or authorization code not found for loan {LoanId}", loanId);
                throw new AppException("Remita customer information not found. Cannot reconcile.", 404);
            }

            // Fetch mandate history from Remita API using the same authorization code from salary history
            _logger.LogInformation("Fetching mandate history from Remita for MandateRef: {MandateRef}", loan.MandateRef);
            var mandateHistory = await _remitaService.GetMandateHistoryAsync(
                salaryHistory.CustomerId,
                loan.MandateRef,
                salaryHistory.AuthorisationCode
            );

            if (mandateHistory == null || mandateHistory.Status?.ToLower() != "success")
            {
                var errorMessage = mandateHistory?.ResponseMsg ?? "Failed to fetch mandate history from Remita";
                _logger.LogError("Mandate history fetch failed for loan {LoanId}: {Error}", loanId, errorMessage);
                throw new AppException($"Reconciliation failed: {errorMessage}", 400);
            }

            // Get local repayment record
            var repayment = await _db.Repayments
                .FirstOrDefaultAsync(r => r.LoanId == loanId) ?? throw new AppException("Repayment record not found for this loan", 404);

            // Get all local webhook notifications for this mandate
            var localNotifications = await _db.RemitaLoanCollectionNotifications
                .Where(n => n.MandateRef == loan.MandateRef)
                .OrderBy(n => n.PaymentDate)
                .ToListAsync();

            // Calculate totals from Remita mandate history
            decimal remitaTotalCollected = 0;
            int remitaPaymentCount = 0;

            if (mandateHistory.Data != null)
            {
                // Try to get total collected from the response
                try
                {
                    var dataJson = JsonSerializer.Serialize(mandateHistory.Data);
                    var dataDict = JsonSerializer.Deserialize<Dictionary<string, object>>(dataJson);

                    if (dataDict != null && dataDict.ContainsKey("totalAmountCollected"))
                    {
                        remitaTotalCollected = Convert.ToDecimal(dataDict["totalAmountCollected"]);
                    }

                    if (dataDict != null && dataDict.ContainsKey("paymentDetails"))
                    {
                        var paymentsJson = dataDict["paymentDetails"].ToString();
                        var payments = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(paymentsJson ?? "[]");
                        remitaPaymentCount = payments?.Count ?? 0;

                        // If total not provided, calculate from payment details
                        if (remitaTotalCollected == 0 && payments != null)
                        {
                            foreach (var payment in payments)
                            {
                                if (payment.ContainsKey("amount"))
                                {
                                    remitaTotalCollected += Convert.ToDecimal(payment["amount"]);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing mandate history data for loan {LoanId}", loanId);
                }
            }

            // Calculate local totals
            var localTotalFromNotifications = localNotifications
                .Where(n => n.PaymentStatus?.Equals("successful", StringComparison.OrdinalIgnoreCase) == true)
                .Sum(n => n.Amount);
            var localPaymentCount = localNotifications.Count(n => n.PaymentStatus?.Equals("successful", StringComparison.OrdinalIgnoreCase) == true);

            // Compare and identify discrepancies
            var discrepancy = remitaTotalCollected - repayment.TotalRepaid;
            var isReconciled = Math.Abs(discrepancy) < 0.01m; // Allow for minor rounding differences

            _logger.LogInformation("Reconciliation comparison for loan {LoanId}: Remita={RemitaTotal}, Local={LocalTotal}, Discrepancy={Discrepancy}",
                loanId, remitaTotalCollected, repayment.TotalRepaid, discrepancy);

            // If there's a discrepancy, update local record to match Remita (source of truth)
            if (!isReconciled && discrepancy > 0)
            {
                _logger.LogInformation("Updating local repayment record to match Remita for loan {LoanId}", loanId);

                var oldTotalRepaid = repayment.TotalRepaid;
                repayment.TotalRepaid = remitaTotalCollected;
                repayment.AmountUnpaid = repayment.TotalDue - repayment.TotalRepaid;
                repayment.LastPaymentAt = DateTime.UtcNow;

                // Update status
                if (repayment.AmountUnpaid <= 0)
                {
                    repayment.Status = RepaymentStatus.Completed;
                    repayment.AmountUnpaid = 0;
                    loan.Status = LoanStatus.Repaid;
                    _db.Loans.Update(loan);
                }
                else if (loan.DueDate.HasValue && DateTime.UtcNow > loan.DueDate.Value)
                {
                    repayment.Status = RepaymentStatus.Overdue;
                    loan.Status = LoanStatus.Overdue;
                    _db.Loans.Update(loan);
                }
                else
                {
                    repayment.Status = RepaymentStatus.Active;
                }

                _db.Repayments.Update(repayment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Reconciliation complete for loan {LoanId}. Updated TotalRepaid from {OldTotal} to {NewTotal}",
                    loanId, oldTotalRepaid, repayment.TotalRepaid);
            }

            return new LoanReconciliationResponseDto
            {
                LoanId = loanId,
                MandateRef = loan.MandateRef,
                IsReconciled = isReconciled,
                RemitaTotalCollected = remitaTotalCollected,
                LocalTotalRepaid = repayment.TotalRepaid,
                Discrepancy = Math.Abs(discrepancy),
                RemitaPaymentCount = remitaPaymentCount,
                LocalPaymentCount = localPaymentCount,
                TotalDue = repayment.TotalDue,
                AmountUnpaid = repayment.AmountUnpaid,
                RepaymentStatus = repayment.Status.ToString(),
                LoanStatus = loan.Status.ToString(),
                ReconciledAt = DateTime.UtcNow,
                ReconciledBy = reconciledBy,
                Message = isReconciled
                    ? "Loan repayment is fully reconciled with Remita records."
                    : $"Reconciliation completed. {(discrepancy > 0 ? "Local record updated to match Remita." : "Discrepancy identified but no action taken.")}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hybrid reconciliation for loan {LoanId}", loanId);
            throw;
        }
    }

    #endregion
}
