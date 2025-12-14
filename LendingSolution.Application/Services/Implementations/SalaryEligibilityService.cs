using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace LendingSolution.Application.Services.Implementations;

public class SalaryEligibilityService : ISalaryEligibilityService
{
    private readonly IRemitaSalaryHistoryRepository _salaryHistoryRepository;
    private readonly ILogger<SalaryEligibilityService> _logger;

    public SalaryEligibilityService(
        IRemitaSalaryHistoryRepository salaryHistoryRepository,
        ILogger<SalaryEligibilityService> logger)
    {
        _salaryHistoryRepository = salaryHistoryRepository;
        _logger = logger;
    }

    public Task<SalaryEligibilityDto> CalculateLoanEligibilityAsync(RemitaSalaryHistoryResponseDto salaryData, LoanProduct product)
    {
        var eligibility = new SalaryEligibilityDto();

        if (salaryData?.Data?.SalaryPaymentDetails == null || !salaryData.Data.SalaryPaymentDetails.Any())
        {
            eligibility.EligibilityReason = "No salary history found";
            eligibility.FinalMinEligible = product.MinAmount;
            eligibility.FinalMaxEligible = product.MinAmount; // Restrict to minimum if no salary data
            return Task.FromResult(eligibility);
        }

        var payments = salaryData.Data.SalaryPaymentDetails;
        var amounts = new List<decimal>();
        var paymentDates = new List<DateTime>();

        // Parse payment amounts and dates
        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, out var amount))
            {
                amounts.Add(amount);
            }

            var paymentDate = ParseRemitaDate(payment.PaymentDate);
            if (paymentDate.HasValue)
            {
                paymentDates.Add(paymentDate.Value);
            }
        }

        if (!amounts.Any())
        {
            eligibility.EligibilityReason = "No valid salary amounts found";
            eligibility.FinalMinEligible = product.MinAmount;
            eligibility.FinalMaxEligible = product.MinAmount;
            return Task.FromResult(eligibility);
        }

        // Calculate salary statistics
        eligibility.AverageMonthlySalary = amounts.Average();
        eligibility.LatestSalaryAmount = amounts.First(); // Assuming first is latest
        eligibility.ConsistentMonths = amounts.Count;

        // Check for outstanding loans
        var loanHistories = salaryData.Data.LoanHistoryDetails ?? new List<RemitaLoanHistoryDto>();
        eligibility.HasOutstandingLoans = loanHistories.Any(l => l.OutstandingAmount > 0);
        eligibility.TotalOutstandingAmount = loanHistories.Sum(l => l.OutstandingAmount);

        // Calculate eligibility based on salary
        var salaryMultiplier = GetSalaryMultiplier(eligibility.ConsistentMonths, eligibility.HasOutstandingLoans);
        
        // Base calculation: percentage of average monthly salary * number of months
        var baseEligibleAmount = eligibility.AverageMonthlySalary * salaryMultiplier;

        // Adjust for outstanding loans (reduce eligible amount)
        if (eligibility.HasOutstandingLoans)
        {
            var monthlyDebtService = eligibility.TotalOutstandingAmount / 12; // Assume 12 months repayment
            var adjustedSalary = eligibility.AverageMonthlySalary - monthlyDebtService;
            baseEligibleAmount = Math.Max(0, adjustedSalary * salaryMultiplier * 0.8m); // 80% of adjusted capacity
        }

        eligibility.CalculatedMinEligible = Math.Max(product.MinAmount, baseEligibleAmount * 0.3m); // 30% of calculated
        eligibility.CalculatedMaxEligible = Math.Min(product.MaxAmount, baseEligibleAmount); // Full calculated amount but capped by product

        // Apply product constraints
        eligibility.FinalMinEligible = Math.Max(product.MinAmount, eligibility.CalculatedMinEligible);
        eligibility.FinalMaxEligible = Math.Min(product.MaxAmount, eligibility.CalculatedMaxEligible);

        // Ensure min doesn't exceed max
        if (eligibility.FinalMinEligible > eligibility.FinalMaxEligible)
        {
            eligibility.FinalMaxEligible = eligibility.FinalMinEligible;
        }

        // Generate eligibility reason
        eligibility.EligibilityReason = GenerateEligibilityReason(eligibility, salaryMultiplier);

        return Task.FromResult(eligibility);
    }

    public async Task<RemitaSalaryHistory> SaveSalaryHistoryAsync(Guid borrowerApplicationId, RemitaSalaryHistoryResponseDto salaryData)
    {
        // Check if salary history already exists for this application
        var existingSalaryHistory = await _salaryHistoryRepository.GetByBorrowerApplicationIdAsync(borrowerApplicationId);
        if (existingSalaryHistory != null)
        {
            _logger.LogInformation("Deleting and recreating salary history for borrower application: {ApplicationId}", borrowerApplicationId);
            
            // Delete the existing record to avoid concurrency issues
            await _salaryHistoryRepository.DeleteAsync(existingSalaryHistory.Id);
        }

        // Create new salary history record
        _logger.LogInformation("Creating new salary history for borrower application: {ApplicationId}", borrowerApplicationId);
        var salaryHistory = CreateSalaryHistoryFromResponse(borrowerApplicationId, salaryData);
        return await _salaryHistoryRepository.CreateAsync(salaryHistory);
    }

    public DateTime? ParseRemitaDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // Try different date formats that Remita might use
        var formats = new[]
        {
            "dd-MM-yyyy HH:mm:ss+0000",
            "dd-MM-yyyy HH:mm:ss",
            "dd-MM-yyyy",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd"
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
        }

        // Try general parsing as fallback
        if (DateTime.TryParse(dateString, out var generalResult))
        {
            return generalResult;
        }

        _logger.LogWarning("Could not parse Remita date: {DateString}", dateString);
        return null;
    }

    private decimal GetSalaryMultiplier(int consistentMonths, bool hasOutstandingLoans)
    {
        // Base multiplier based on salary consistency
        decimal baseMultiplier = consistentMonths switch
        {
            >= 6 => 3.0m,  // 3x monthly salary for 6+ months consistency
            >= 4 => 2.5m,  // 2.5x for 4-5 months
            >= 2 => 2.0m,  // 2x for 2-3 months
            _ => 1.5m       // 1.5x for less than 2 months
        };

        // Reduce multiplier if there are outstanding loans
        if (hasOutstandingLoans)
        {
            baseMultiplier *= 0.7m; // Reduce by 30%
        }

        return baseMultiplier;
    }

    private string GenerateEligibilityReason(SalaryEligibilityDto eligibility, decimal multiplier)
    {
        var reasons = new List<string>();

        if (eligibility.ConsistentMonths >= 6)
        {
            reasons.Add($"Excellent salary consistency ({eligibility.ConsistentMonths} months)");
        }
        else if (eligibility.ConsistentMonths >= 3)
        {
            reasons.Add($"Good salary consistency ({eligibility.ConsistentMonths} months)");
        }
        else
        {
            reasons.Add($"Limited salary history ({eligibility.ConsistentMonths} months)");
        }

        reasons.Add($"Average monthly salary: ₦{eligibility.AverageMonthlySalary:N2}");
        reasons.Add($"Applied {multiplier:F1}x salary multiplier");

        if (eligibility.HasOutstandingLoans)
        {
            reasons.Add($"Adjusted for outstanding loans: ₦{eligibility.TotalOutstandingAmount:N2}");
        }

        return string.Join("; ", reasons);
    }

    private RemitaSalaryHistory CreateSalaryHistoryFromResponse(Guid borrowerApplicationId, RemitaSalaryHistoryResponseDto salaryData)
    {
        var data = salaryData.Data!;
        var payments = data.SalaryPaymentDetails ?? new List<RemitaSalaryPaymentDto>();
        var loans = data.LoanHistoryDetails ?? new List<RemitaLoanHistoryDto>();

        var amounts = new List<decimal>();
        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, out var amount))
            {
                amounts.Add(amount);
            }
        }

        var salaryHistory = new RemitaSalaryHistory
        {
            BorrowerApplicationId = borrowerApplicationId,
            CustomerId = data.CustomerId,
            AccountNumber = data.AccountNumber,
            BankCode = data.BankCode,
            BVN = data.BVN,
            CompanyName = data.CompanyName,
            CustomerName = data.CustomerName,
            Category = data.Category,
            FirstPaymentDate = ParseRemitaDate(data.FirstPaymentDate),
            SalaryCount = int.TryParse(data.SalaryCount, out var count) ? count : 0,
            AverageMonthlySalary = amounts.Any() ? amounts.Average() : 0,
            LatestSalaryAmount = amounts.Any() ? amounts.First() : 0,
            LatestPaymentDate = payments.Any() ? ParseRemitaDate(payments.First().PaymentDate) : null,
            MinSalaryAmount = amounts.Any() ? amounts.Min() : 0,
            MaxSalaryAmount = amounts.Any() ? amounts.Max() : 0,
            ConsistentMonths = amounts.Count,
            HasOutstandingLoans = loans.Any(l => l.OutstandingAmount > 0),
            TotalOutstandingAmount = loans.Sum(l => l.OutstandingAmount),
            RawRemitaResponse = JsonSerializer.Serialize(salaryData)
        };

        // Add salary payments
        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, out var amount))
            {
                salaryHistory.SalaryPayments.Add(new RemitaSalaryPayment
                {
                    PaymentDate = ParseRemitaDate(payment.PaymentDate) ?? DateTime.UtcNow,
                    Amount = amount,
                    AccountNumber = payment.AccountNumber,
                    BankCode = payment.BankCode
                });
            }
        }

        // Add loan histories
        foreach (var loan in loans)
        {
            salaryHistory.LoanHistories.Add(new RemitaLoanHistory
            {
                LoanProvider = loan.LoanProvider,
                LoanAmount = loan.LoanAmount,
                OutstandingAmount = loan.OutstandingAmount,
                LoanDisbursementDate = ParseRemitaDate(loan.LoanDisbursementDate),
                Status = loan.Status,
                RepaymentAmount = loan.RepaymentAmount,
                RepaymentFreq = loan.RepaymentFreq
            });
        }

        return salaryHistory;
    }

    private void UpdateSalaryHistoryFromResponse(RemitaSalaryHistory existingHistory, RemitaSalaryHistoryResponseDto salaryData)
    {
        var data = salaryData.Data!;
        var payments = data.SalaryPaymentDetails ?? new List<RemitaSalaryPaymentDto>();
        var loans = data.LoanHistoryDetails ?? new List<RemitaLoanHistoryDto>();

        var amounts = new List<decimal>();
        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, out var amount))
            {
                amounts.Add(amount);
            }
        }

        // Update main properties
        existingHistory.CustomerId = data.CustomerId;
        existingHistory.CompanyName = data.CompanyName;
        existingHistory.CustomerName = data.CustomerName;
        existingHistory.Category = data.Category;
        existingHistory.FirstPaymentDate = ParseRemitaDate(data.FirstPaymentDate);
        existingHistory.SalaryCount = int.TryParse(data.SalaryCount, out var count) ? count : 0;
        existingHistory.AverageMonthlySalary = amounts.Any() ? amounts.Average() : 0;
        existingHistory.LatestSalaryAmount = amounts.Any() ? amounts.First() : 0;
        existingHistory.LatestPaymentDate = payments.Any() ? ParseRemitaDate(payments.First().PaymentDate) : null;
        existingHistory.MinSalaryAmount = amounts.Any() ? amounts.Min() : 0;
        existingHistory.MaxSalaryAmount = amounts.Any() ? amounts.Max() : 0;
        existingHistory.ConsistentMonths = amounts.Count;
        existingHistory.HasOutstandingLoans = loans.Any(l => l.OutstandingAmount > 0);
        existingHistory.TotalOutstandingAmount = loans.Sum(l => l.OutstandingAmount);
        existingHistory.RawRemitaResponse = JsonSerializer.Serialize(salaryData);

        // Add new salary payments
        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, out var amount))
            {
                existingHistory.SalaryPayments.Add(new RemitaSalaryPayment
                {
                    PaymentDate = ParseRemitaDate(payment.PaymentDate) ?? DateTime.UtcNow,
                    Amount = amount,
                    AccountNumber = payment.AccountNumber,
                    BankCode = payment.BankCode
                });
            }
        }

        // Add new loan histories
        foreach (var loan in loans)
        {
            existingHistory.LoanHistories.Add(new RemitaLoanHistory
            {
                LoanProvider = loan.LoanProvider,
                LoanAmount = loan.LoanAmount,
                OutstandingAmount = loan.OutstandingAmount,
                LoanDisbursementDate = ParseRemitaDate(loan.LoanDisbursementDate),
                Status = loan.Status,
                RepaymentAmount = loan.RepaymentAmount,
                RepaymentFreq = loan.RepaymentFreq
            });
        }
    }
}
