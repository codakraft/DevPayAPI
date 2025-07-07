using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

public class FinanceService : IFinanceService
{
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<FinanceService> _logger;

    public FinanceService(
        IDisbursementRepository disbursementRepository,
        IRepaymentRepository repaymentRepository,
        ILoanRepository loanRepository,
        ICompanyRepository companyRepository,
        ILogger<FinanceService> logger)
    {
        _disbursementRepository = disbursementRepository;
        _repaymentRepository = repaymentRepository;
        _loanRepository = loanRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<ApiResponse> GetAllDisbursementsAsync()
    {
        try
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

            return ApiResponse.Ok("Disbursements retrieved successfully", disbursementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving disbursements");
            return ApiResponse.Fail("Failed to retrieve disbursements");
        }
    }

    public async Task<ApiResponse> GetAllRepaymentsAsync()
    {
        try
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

            return ApiResponse.Ok("Repayments retrieved successfully", repaymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving repayments");
            return ApiResponse.Fail("Failed to retrieve repayments");
        }
    }

    public async Task<ApiResponse> ProcessDisbursementAsync(string loanId, DisbursementRequestDto request, string? processedBy = null)
    {
        try
        {
            // Validate loan exists and is approved
            var loan = await _loanRepository.GetLoanById(Guid.Parse(loanId));
            if (loan == null)
            {
                return ApiResponse.Fail("Loan not found");
            }

            if (loan.Status != Core.Enum.LoanStatus.Approved)
            {
                return ApiResponse.Fail("Loan must be approved before disbursement");
            }

            // Check if disbursement already exists for this loan
            var existingDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
            var totalDisbursed = existingDisbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
            
            if (totalDisbursed + request.Amount > loan.Amount)
            {
                return ApiResponse.Fail("Disbursement amount exceeds loan amount");
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
                return ApiResponse.Fail("Failed to process disbursement");
            }

            var disbursementDto = new DisbursementDto
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

            return ApiResponse.Ok("Disbursement processed successfully", disbursementDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing disbursement for loan {LoanId}", loanId);
            return ApiResponse.Fail("Failed to process disbursement");
        }
    }

    public async Task<ApiResponse> ProcessRepaymentAsync(RepaymentRequestDto request, string? processedBy = null)
    {
        try
        {
            // Validate loan exists
            var loan = await _loanRepository.GetLoanById(Guid.Parse(request.LoanId));
            if (loan == null)
            {
                return ApiResponse.Fail("Loan not found");
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
                return ApiResponse.Fail("Failed to process repayment");
            }

            var repaymentDto = new RepaymentDto
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

            return ApiResponse.Ok("Repayment processed successfully", repaymentDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing repayment for loan {LoanId}", request.LoanId);
            return ApiResponse.Fail("Failed to process repayment");
        }
    }

    public async Task<ApiResponse> GetMonthlyFinanceReportAsync(int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var disbursements = await _disbursementRepository.GetDisbursementsByDateRange(startDate, endDate);
            var repayments = await _repaymentRepository.GetRepaymentsByDateRange(startDate, endDate);

            var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
            var totalRepaid = repayments.Where(r => r.Status == "Verified").Sum(r => r.Amount);

            var report = new FinanceReportDto
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

            return ApiResponse.Ok("Monthly finance report generated successfully", report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating monthly finance report for {Year}-{Month}", year, month);
            return ApiResponse.Fail("Failed to generate monthly finance report");
        }
    }

    public async Task<ApiResponse> GetCompanyFinanceReportAsync(string companyId)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(Guid.Parse(companyId));
            if (company == null)
            {
                return ApiResponse.Fail("Company not found");
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

            var report = new FinanceReportDto
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

            return ApiResponse.Ok("Company finance report generated successfully", report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating finance report for company {CompanyId}", companyId);
            return ApiResponse.Fail("Failed to generate company finance report");
        }
    }

    public async Task<ApiResponse> GetCompanyWalletAsync(string companyId)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(Guid.Parse(companyId));
            if (company == null)
            {
                return ApiResponse.Fail("Company not found");
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

            var wallet = new CompanyWalletDto
            {
                CompanyId = companyId,
                CompanyName = company.Name,
                AvailableBalance = availableBalance,
                PendingDisbursements = pendingDisbursements,
                TotalReceived = totalReceived,
                LastUpdated = DateTime.UtcNow
            };

            return ApiResponse.Ok("Company wallet retrieved successfully", wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving wallet for company {CompanyId}", companyId);
            return ApiResponse.Fail("Failed to retrieve company wallet");
        }
    }

    public async Task<ApiResponse> GetDisbursementsByLoanIdAsync(string loanId)
    {
        try
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

            return ApiResponse.Ok("Loan disbursements retrieved successfully", disbursementDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving disbursements for loan {LoanId}", loanId);
            return ApiResponse.Fail("Failed to retrieve loan disbursements");
        }
    }

    public async Task<ApiResponse> GetRepaymentsByLoanIdAsync(string loanId)
    {
        try
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

            return ApiResponse.Ok("Loan repayments retrieved successfully", repaymentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving repayments for loan {LoanId}", loanId);
            return ApiResponse.Fail("Failed to retrieve loan repayments");
        }
    }
}
