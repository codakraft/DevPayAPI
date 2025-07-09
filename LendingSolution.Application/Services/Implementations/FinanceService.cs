using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class FinanceService(
    IDisbursementRepository disbursementRepository,
    IRepaymentRepository repaymentRepository,
    ILoanRepository loanRepository,
    ICompanyRepository companyRepository,
    ILogger<FinanceService> logger) : IFinanceService
{
    private readonly IDisbursementRepository _disbursementRepository = disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository = repaymentRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILogger<FinanceService> _logger = logger;

    public async Task<List<DisbursementDto>> GetAllDisbursementsAsync()
    {
        var disbursements = await _disbursementRepository.GetAllDisbursements();
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

    public async Task<List<RepaymentDto>> GetAllRepaymentsAsync()
    {
        var repayments = await _repaymentRepository.GetAllRepayments();
        var repaymentDtos = repayments.Select(r => new RepaymentDto
        {
            Id = r.Id.ToString(),
            LoanId = r.LoanId,
            Amount = r.Amount,
            PaymentMethod = r.PaymentMethod,
            Status = r.Status,
            PaymentReference = r.PaymentReference,
            CreatedAt = r.CreatedAt,
            ProcessedAt = r.ProcessedAt,
            ProcessedBy = r.ProcessedBy,
            Notes = r.Notes
        }).ToList();

        return repaymentDtos;
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

    public async Task<RepaymentDto> ProcessRepaymentAsync(RepaymentRequestDto request, string? processedBy = null)
    {
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
    }

    public async Task<FinanceReportDto> GetMonthlyFinanceReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var disbursements = await _disbursementRepository.GetDisbursementsByDateRange(startDate, endDate);
        var repayments = await _repaymentRepository.GetRepaymentsByDateRange(startDate, endDate);

        var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
        var totalRepaid = repayments.Where(r => r.Status == "Verified").Sum(r => r.Amount);

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
        var totalRepaid = repayments.Where(r => r.Status == "Verified").Sum(r => r.Amount);

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

        var totalReceived = repayments.Where(r => r.Status == "Verified").Sum(r => r.Amount);
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
            Amount = r.Amount,
            PaymentMethod = r.PaymentMethod,
            Status = r.Status,
            PaymentReference = r.PaymentReference,
            CreatedAt = r.CreatedAt,
            ProcessedAt = r.ProcessedAt,
            ProcessedBy = r.ProcessedBy,
            Notes = r.Notes
        }).ToList();

        return repaymentDtos;
    }
}
