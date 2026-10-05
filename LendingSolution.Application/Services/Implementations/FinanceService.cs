using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;
using LendingSolution.Application.Exceptions;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Implementations;

public class FinanceService(
    IDisbursementRepository disbursementRepository,
    IRepaymentRepository repaymentRepository,
    ILoanRepository loanRepository,
    ICompanyRepository companyRepository,
    ApplicationDbContext db,
    ILogger<FinanceService> logger) : IFinanceService
{
    private readonly IDisbursementRepository _disbursementRepository = disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository = repaymentRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<FinanceService> _logger = logger;

    public async Task<PagedDisbursementListDto> GetDisbursementsAsync(Guid? companyId, int page, int pageSize)
    {
        var query = _db.Disbursements.AsNoTracking().OrderByDescending(d => d.CreatedAt);

        int totalCount;
        List<Disbursement> pageItems;

        if (companyId.HasValue)
        {
            // Disbursement.LoanId is a string, so match it to the company's loans as GUIDs in memory
            var companyLoanIds = (await _db.Loans
                    .Where(l => l.CompanyId == companyId.Value)
                    .Select(l => l.Id)
                    .ToListAsync())
                .ToHashSet();

            var companyDisbursements = (await query.ToListAsync())
                .Where(d => Guid.TryParse(d.LoanId, out var loanId) && companyLoanIds.Contains(loanId))
                .ToList();

            totalCount = companyDisbursements.Count;
            pageItems = companyDisbursements.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        else
        {
            totalCount = await query.CountAsync();
            pageItems = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedDisbursementListDto
        {
            Disbursements = pageItems.Select(d => new DisbursementDto
            {
                Id = d.Id.ToString(),
                LoanId = d.LoanId,
                Amount = d.Amount,
                Status = d.Status,
                AccountDetails = d.AccountDetails,
                DisbursementMethod = d.DisbursementMethod,
                RequestedAt = d.RequestedAt,
                ProcessedAt = d.ProcessedAt,
                ProcessedBy = d.ProcessedBy,
                Notes = d.Notes
            }).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<PagedRepaymentDto> GetAllRepaymentsAsync(RepaymentFilterDto filters)
    {
        var query = _db.Repayments
            .Include(r => r.Loan)
                .ThenInclude(l => l.BorrowerApplication)
            .Include(r => r.Loan)
                .ThenInclude(l => l.Company)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            query = query.Where(r =>
                (r.Loan.BorrowerApplication != null &&
                    (r.Loan.BorrowerApplication.FirstName.ToLower().Contains(searchLower) ||
                     r.Loan.BorrowerApplication.LastName.ToLower().Contains(searchLower) ||
                     r.Loan.BorrowerApplication.Email.ToLower().Contains(searchLower))) ||
                r.LoanId.ToString().Contains(searchLower)
            );
        }

        // Apply company filter
        if (filters.CompanyId.HasValue)
        {
            query = query.Where(r => r.Loan.CompanyId == filters.CompanyId.Value);
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            if (Enum.TryParse<RepaymentStatus>(filters.Status, true, out var status))
            {
                query = query.Where(r => r.Status == status);
            }
        }

        // Apply amount filters
        if (filters.MinTotalDue.HasValue)
        {
            query = query.Where(r => r.TotalDue >= filters.MinTotalDue.Value);
        }

        if (filters.MaxTotalDue.HasValue)
        {
            query = query.Where(r => r.TotalDue <= filters.MaxTotalDue.Value);
        }

        if (filters.MinAmountUnpaid.HasValue)
        {
            query = query.Where(r => r.AmountUnpaid >= filters.MinAmountUnpaid.Value);
        }

        if (filters.MaxAmountUnpaid.HasValue)
        {
            query = query.Where(r => r.AmountUnpaid <= filters.MaxAmountUnpaid.Value);
        }

        // Apply date filters
        if (filters.CreatedAfter.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= filters.CreatedAfter.Value);
        }

        if (filters.CreatedBefore.HasValue)
        {
            query = query.Where(r => r.CreatedAt <= filters.CreatedBefore.Value);
        }

        if (filters.LastPaymentAfter.HasValue)
        {
            query = query.Where(r => r.LastPaymentAt >= filters.LastPaymentAfter.Value);
        }

        if (filters.LastPaymentBefore.HasValue)
        {
            query = query.Where(r => r.LastPaymentAt <= filters.LastPaymentBefore.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = filters.SortBy?.ToLower() switch
        {
            "totaldue" => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.TotalDue)
                : query.OrderByDescending(r => r.TotalDue),
            "totalrepaid" => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.TotalRepaid)
                : query.OrderByDescending(r => r.TotalRepaid),
            "amountunpaid" => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.AmountUnpaid)
                : query.OrderByDescending(r => r.AmountUnpaid),
            "lastpaymentat" => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.LastPaymentAt)
                : query.OrderByDescending(r => r.LastPaymentAt),
            "status" => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.Status)
                : query.OrderByDescending(r => r.Status),
            _ => filters.SortOrder?.ToLower() == "asc"
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt)
        };

        // Apply pagination
        var skip = (filters.Page - 1) * filters.PageSize;
        var repayments = await query
            .Skip(skip)
            .Take(filters.PageSize)
            .ToListAsync();

        // Calculate summary statistics from all filtered results (not just current page)
        // Re-apply the same filters to get statistics
        var statsQuery = _db.Repayments
            .Include(r => r.Loan)
                .ThenInclude(l => l.BorrowerApplication)
            .Include(r => r.Loan)
                .ThenInclude(l => l.Company)
            .AsQueryable();

        // Apply the same filters as before
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchLower = filters.Search.ToLower();
            statsQuery = statsQuery.Where(r =>
                (r.Loan.BorrowerApplication != null &&
                    (r.Loan.BorrowerApplication.FirstName.ToLower().Contains(searchLower) ||
                     r.Loan.BorrowerApplication.LastName.ToLower().Contains(searchLower) ||
                     r.Loan.BorrowerApplication.Email.ToLower().Contains(searchLower))) ||
                r.LoanId.ToString().Contains(searchLower)
            );
        }

        if (filters.CompanyId.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.Loan.CompanyId == filters.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            if (Enum.TryParse<RepaymentStatus>(filters.Status, true, out var status))
            {
                statsQuery = statsQuery.Where(r => r.Status == status);
            }
        }

        if (filters.MinTotalDue.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.TotalDue >= filters.MinTotalDue.Value);
        }

        if (filters.MaxTotalDue.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.TotalDue <= filters.MaxTotalDue.Value);
        }

        if (filters.MinAmountUnpaid.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.AmountUnpaid >= filters.MinAmountUnpaid.Value);
        }

        if (filters.MaxAmountUnpaid.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.AmountUnpaid <= filters.MaxAmountUnpaid.Value);
        }

        if (filters.CreatedAfter.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.CreatedAt >= filters.CreatedAfter.Value);
        }

        if (filters.CreatedBefore.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.CreatedAt <= filters.CreatedBefore.Value);
        }

        if (filters.LastPaymentAfter.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.LastPaymentAt >= filters.LastPaymentAfter.Value);
        }

        if (filters.LastPaymentBefore.HasValue)
        {
            statsQuery = statsQuery.Where(r => r.LastPaymentAt <= filters.LastPaymentBefore.Value);
        }

        var allFilteredRepayments = await statsQuery.ToListAsync();

        var totalDueAmount = allFilteredRepayments.Sum(r => r.TotalDue);
        var totalRepaidAmount = allFilteredRepayments.Sum(r => r.TotalRepaid);
        var totalUnpaidAmount = allFilteredRepayments.Sum(r => r.AmountUnpaid);
        var activeCount = allFilteredRepayments.Count(r => r.Status == RepaymentStatus.Active);
        var overdueCount = allFilteredRepayments.Count(r => r.Status == RepaymentStatus.Overdue);
        var completedCount = allFilteredRepayments.Count(r => r.Status == RepaymentStatus.Completed);

        // Map to DTOs
        var repaymentDtos = repayments.Select(r => new RepaymentDto
        {
            Id = r.Id.ToString(),
            LoanId = r.LoanId,
            TotalDue = r.TotalDue,
            TotalRepaid = r.TotalRepaid,
            LastPaymentAt = r.LastPaymentAt,
            AmountUnpaid = r.AmountUnpaid,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt,
            BorrowerName = r.Loan.BorrowerApplication != null
                ? $"{r.Loan.BorrowerApplication.FirstName} {r.Loan.BorrowerApplication.LastName}"
                : null,
            BorrowerEmail = r.Loan.BorrowerApplication?.Email,
            CompanyName = r.Loan.Company?.Name
        }).ToList();

        // Calculate pagination info
        var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);

        return new PagedRepaymentDto
        {
            Repayments = repaymentDtos,
            TotalCount = totalCount,
            Page = filters.Page,
            PageSize = filters.PageSize,
            TotalPages = totalPages,
            HasNextPage = filters.Page < totalPages,
            HasPreviousPage = filters.Page > 1,
            TotalDueAmount = totalDueAmount,
            TotalRepaidAmount = totalRepaidAmount,
            TotalUnpaidAmount = totalUnpaidAmount,
            ActiveCount = activeCount,
            OverdueCount = overdueCount,
            CompletedCount = completedCount
        };
    }

    public async Task<DisbursementDto> ProcessDisbursementAsync(string loanId, DisbursementRequestDto request, string? processedBy = null)
    {
        // Validate loan exists and is approved
        var loan = await _loanRepository.GetLoanById(Guid.Parse(loanId));
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        if (loan.Status != Core.Enum.LoanStatus.Approved)
        {
            throw new AppException("Loan must be approved before disbursement", 400);
        }

        // Check if disbursement already exists for this loan
        var existingDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
        var totalDisbursed = existingDisbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
        
        if (totalDisbursed + request.Amount > loan.Amount)
        {
            throw new AppException("Disbursement amount exceeds loan amount", 400);
        }

        var disbursement = new Disbursement
        {
            LoanId = loanId,
            Amount = request.Amount,
            AccountDetails = request.AccountDetails,
            DisbursementMethod = request.DisbursementMethod,
            Status = "Disbursed", // Auto-approve for now
            RequestedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow,
            ProcessedBy = processedBy,
            Notes = request.Notes
        };

        var result = await _disbursementRepository.CreateDisbursement(disbursement);
        if (!result)
        {
            throw new AppException("Failed to process disbursement", 500);
        }

        return new DisbursementDto
        {
            Id = disbursement.Id.ToString(),
            LoanId = disbursement.LoanId,
            Amount = disbursement.Amount,
            Status = disbursement.Status,
            AccountDetails = disbursement.AccountDetails,
            DisbursementMethod = disbursement.DisbursementMethod,
            RequestedAt = disbursement.RequestedAt,
            ProcessedAt = disbursement.ProcessedAt,
            ProcessedBy = disbursement.ProcessedBy,
            Notes = disbursement.Notes
        };
    }

    // TODO: ProcessRepaymentAsync needs to be redesigned for new Repayment schema
    // The new schema tracks overall loan repayment status, not individual payment transactions
    public async Task<RepaymentDto> ProcessRepaymentAsync(RepaymentRequestDto request, string? processedBy = null)
    {
        throw new NotImplementedException("ProcessRepaymentAsync needs to be redesigned for new Repayment schema");
        
        /*
        // Validate loan exists
        var loan = await _loanRepository.GetLoanById(Guid.Parse(request.LoanId));
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        var repayment = new Repayment
        {
            LoanId = request.LoanId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            PaymentReference = request.PaymentReference,
            Status = "Verified", // Auto-verify for now
            ProcessedAt = DateTime.UtcNow,
            ProcessedBy = processedBy,
            Notes = request.Notes
        };

        var result = await _repaymentRepository.CreateRepayment(repayment);
        if (!result)
        {
            throw new AppException("Failed to process repayment", 500);
        }

        return new RepaymentDto
        {
            Id = repayment.Id.ToString(),
            LoanId = repayment.LoanId,
            Amount = repayment.Amount,
            PaymentMethod = repayment.PaymentMethod,
            Status = repayment.Status,
            PaymentReference = repayment.PaymentReference,
            CreatedAt = repayment.CreatedAt,
            ProcessedAt = repayment.ProcessedAt,
            ProcessedBy = repayment.ProcessedBy,
            Notes = repayment.Notes
        };
        */
    }

    public async Task<FinanceReportDto> GetMonthlyFinanceReportAsync(int year, int month, Guid? companyId = null)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var disbursements = await _disbursementRepository.GetDisbursementsByDateRange(startDate, endDate);
        var repayments = await _repaymentRepository.GetRepaymentsByDateRange(startDate, endDate);

        // Filter by company if provided
        if (companyId.HasValue)
        {
            var companyLoanIds = await _db.Loans
                .Where(l => l.CompanyId == companyId.Value)
                .Select(l => l.Id.ToString())
                .ToListAsync();

            disbursements = disbursements.Where(d => companyLoanIds.Contains(d.LoanId)).ToList();
            
            var companyLoanGuids = companyLoanIds.Select(id => Guid.Parse(id)).ToList();
            repayments = repayments.Where(r => companyLoanGuids.Contains(r.LoanId)).ToList();
        }

        var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
        var totalRepaid = repayments.Sum(r => r.TotalRepaid);

        return new FinanceReportDto
        {
            TotalDisbursed = totalDisbursed,
            TotalRepaid = totalRepaid,
            OutstandingAmount = totalDisbursed - totalRepaid,
            TotalLoans = disbursements.GroupBy(d => d.LoanId).Count(),
            ActiveLoans = disbursements.Where(d => d.Status == "Disbursed").GroupBy(d => d.LoanId).Count(),
            CompletedLoans = 0, // Would need additional logic to determine completed loans
            ReportPeriodStart = startDate,
            ReportPeriodEnd = endDate
        };
    }

    public async Task<FinanceReportDto> GetCompanyFinanceReportAsync(string companyId)
    {
        var company = await _companyRepository.GetCompanyById(Guid.Parse(companyId));
        if (company == null)
        {
            throw new AppException("Company not found", 404);
        }

        var companyLoans = await _loanRepository.GetAllLoansByCompanyId(Guid.Parse(companyId));
        var loanIds = companyLoans.Select(l => l.Id.ToString()).ToList();

        var disbursements = new List<Disbursement>();
        var repayments = new List<Repayment>();

        foreach (var loanId in loanIds)
        {
            var loanDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
            var loanRepayments = await _repaymentRepository.GetRepaymentsByLoanId(loanId);
            
            disbursements.AddRange(loanDisbursements);
            repayments.AddRange(loanRepayments);
        }

        var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
        var totalRepaid = repayments.Sum(r => r.TotalRepaid);

        return new FinanceReportDto
        {
            TotalDisbursed = totalDisbursed,
            TotalRepaid = totalRepaid,
            OutstandingAmount = totalDisbursed - totalRepaid,
            TotalLoans = companyLoans.Count(),
            ActiveLoans = companyLoans.Count(l => l.Status == Core.Enum.LoanStatus.Approved),
            CompletedLoans = companyLoans.Count(l => l.Status == Core.Enum.LoanStatus.Repaid),
            ReportPeriodStart = companyLoans.Min(l => l.CreatedAt),
            ReportPeriodEnd = DateTime.UtcNow
        };
    }

    public async Task<CompanyWalletDto> GetCompanyWalletAsync(string companyId)
    {
        var company = await _companyRepository.GetCompanyById(Guid.Parse(companyId));
        if (company == null)
        {
            throw new AppException("Company not found", 404);
        }

        var companyLoans = await _loanRepository.GetAllLoansByCompanyId(Guid.Parse(companyId));
        var loanIds = companyLoans.Select(l => l.Id.ToString()).ToList();

        var disbursements = new List<Disbursement>();
        var repayments = new List<Repayment>();

        foreach (var loanId in loanIds)
        {
            var loanDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
            var loanRepayments = await _repaymentRepository.GetRepaymentsByLoanId(loanId);
            
            disbursements.AddRange(loanDisbursements);
            repayments.AddRange(loanRepayments);
        }

        var totalReceived = repayments.Sum(r => r.TotalRepaid);
        var pendingDisbursements = disbursements.Where(d => d.Status == "Pending").Sum(d => d.Amount);
        var availableBalance = totalReceived - pendingDisbursements;

        return new CompanyWalletDto
        {
            CompanyId = companyId,
            CompanyName = company.Name,
            AvailableBalance = availableBalance,
            PendingDisbursements = pendingDisbursements,
            TotalReceived = totalReceived,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<List<DisbursementDto>> GetDisbursementsByLoanIdAsync(string loanId)
    {
        var disbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
        var disbursementDtos = disbursements.Select(d => new DisbursementDto
        {
            Id = d.Id.ToString(),
            LoanId = d.LoanId,
            Amount = d.Amount,
            Status = d.Status,
            AccountDetails = d.AccountDetails,
            DisbursementMethod = d.DisbursementMethod,
            RequestedAt = d.RequestedAt,
            ProcessedAt = d.ProcessedAt,
            ProcessedBy = d.ProcessedBy,
            Notes = d.Notes
        }).ToList();

        return disbursementDtos;
    }

    public async Task<List<RepaymentDto>> GetRepaymentsByLoanIdAsync(string loanId)
    {
        var repayments = await _repaymentRepository.GetRepaymentsByLoanId(loanId);
        var repaymentDtos = repayments.Select(r => new RepaymentDto
        {
            Id = r.Id.ToString(),
            LoanId = r.LoanId,
            TotalDue = r.TotalDue,
            TotalRepaid = r.TotalRepaid,
            LastPaymentAt = r.LastPaymentAt,
            AmountUnpaid = r.AmountUnpaid,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return repaymentDtos;
    }
}
