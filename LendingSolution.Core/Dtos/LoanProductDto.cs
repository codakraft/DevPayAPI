using LendingSolution.Core.Enum;
using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class LoanProductDto
{
    public required Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    
    // Amount and Tenor ranges
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    
    // Interest and fees
    public decimal InterestRate { get; set; } = 0;
    public decimal PenaltyOnDefaultPrincipal { get; set; } = 0;
    public decimal ProcessingFeePercent { get; set; } = 0;
    public decimal ProcessingFeeFlat { get; set; } = 0;
    public decimal MaintenanceFeePercent { get; set; } = 1;
    public decimal LegalFeePercent { get; set; } = 0;
    public decimal LegalFeeFlat { get; set; } = 0;
    public decimal EligibilityPercentage { get; set; } = 33;
    
    // Eligibility and settings
    public int Moratorium { get; set; } = 30; // in days
    public bool NotifyApprovalsViaEmail { get; set; } = false;
    public decimal TurnoverEligibilityPercent { get; set; } = 0;
    
    // Interest computation settings
    public InterestComputationBasis InterestComputationBasis { get; set; } = InterestComputationBasis.Flat;
    public InterestCostComputation InterestCostComputation { get; set; } = InterestCostComputation.PerMonth;
    
    // Payment schedule settings
    public PaymentScheduleBreakdown PaymentScheduleBreakdown { get; set; } = PaymentScheduleBreakdown.Monthly;
    public PaymentScheduleType PaymentScheduleType { get; set; } = PaymentScheduleType.Fixed;
}

public class CreateLoanProductRequestDto
{
    // CompanyId will be extracted from JWT, so we don't need it in the request
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    
    // Amount and Tenor ranges
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    
    // Interest and fees
    public decimal InterestRate { get; set; } = 0;
    public decimal PenaltyOnDefaultPrincipal { get; set; } = 0;
    public decimal ProcessingFeePercent { get; set; } = 0;
    public decimal ProcessingFeeFlat { get; set; } = 0;
    public decimal MaintenanceFeePercent { get; set; } = 1;
    public decimal LegalFeePercent { get; set; } = 0;
    public decimal LegalFeeFlat { get; set; } = 0;
    public decimal EligibilityPercentage { get; set; } = 33;
    
    // Eligibility and settings
    public int Moratorium { get; set; } = 30; // in days
    public bool NotifyApprovalsViaEmail { get; set; } = false;
    public decimal TurnoverEligibilityPercent { get; set; } = 0;
    
    // Interest computation settings
    public InterestComputationBasis InterestComputationBasis { get; set; } = InterestComputationBasis.Flat;
    public InterestCostComputation InterestCostComputation { get; set; } = InterestCostComputation.PerMonth;
    
    // Payment schedule settings
    public PaymentScheduleBreakdown PaymentScheduleBreakdown { get; set; } = PaymentScheduleBreakdown.Monthly;
    public PaymentScheduleType PaymentScheduleType { get; set; } = PaymentScheduleType.Fixed;
}

public class UpdateLoanProductRequestDto
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    
    // Amount and Tenor ranges
    [Range(0, double.MaxValue, ErrorMessage = "Minimum amount must be a positive value")]
    public decimal MinAmount { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Maximum amount must be a positive value")]
    public decimal MaxAmount { get; set; } = 0;
    
    [Range(1, int.MaxValue, ErrorMessage = "Minimum tenor must be at least 1 month")]
    public int MinTenor { get; set; } = 1;
    
    [Range(1, int.MaxValue, ErrorMessage = "Maximum tenor must be at least 1 month")]
    public int MaxTenor { get; set; } = 12;
    
    // Interest and fees
    public decimal InterestRate { get; set; } = 0;
    public decimal PenaltyOnDefaultPrincipal { get; set; } = 0;
    public decimal ProcessingFeePercent { get; set; } = 0;
    public decimal ProcessingFeeFlat { get; set; } = 0;
    public decimal MaintenanceFeePercent { get; set; } = 1;
    public decimal LegalFeePercent { get; set; } = 0;
    public decimal LegalFeeFlat { get; set; } = 0;
    public decimal EligibilityPercentage { get; set; } = 33;
    
    // Eligibility and settings
    public int Moratorium { get; set; } = 30; // in days
    public bool NotifyApprovalsViaEmail { get; set; } = false;
    public decimal TurnoverEligibilityPercent { get; set; } = 0;
    
    // Interest computation settings
    public InterestComputationBasis InterestComputationBasis { get; set; } = InterestComputationBasis.Flat;
    public InterestCostComputation InterestCostComputation { get; set; } = InterestCostComputation.PerMonth;
    
    // Payment schedule settings
    public PaymentScheduleBreakdown PaymentScheduleBreakdown { get; set; } = PaymentScheduleBreakdown.Monthly;
    public PaymentScheduleType PaymentScheduleType { get; set; } = PaymentScheduleType.Fixed;
}

public class LoanProductResponseDto : LoanProductDto
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LoanProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    
    // Amount and Tenor ranges
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int MinTenor { get; set; }
    public int MaxTenor { get; set; }
    
    // Interest and fees
    public decimal InterestRate { get; set; }
    public decimal PenaltyOnDefaultPrincipal { get; set; }
    public decimal ProcessingFeePercent { get; set; }
    public decimal ProcessingFeeFlat { get; set; }
    public decimal MaintenanceFeePercent { get; set; }
    public decimal LegalFeePercent { get; set; }
    public decimal LegalFeeFlat { get; set; }
    public decimal EligibilityPercentage { get; set; }
    
    // Eligibility and settings
    public int Moratorium { get; set; }
    public bool NotifyApprovalsViaEmail { get; set; }
    public decimal TurnoverEligibilityPercent { get; set; }
    
    // Interest computation settings
    public InterestComputationBasis InterestComputationBasis { get; set; }
    public InterestCostComputation InterestCostComputation { get; set; }
    
    // Payment schedule settings
    public PaymentScheduleBreakdown PaymentScheduleBreakdown { get; set; }
    public PaymentScheduleType PaymentScheduleType { get; set; }
    
    // Status and metadata
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Company Information
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyShortName { get; set; } = string.Empty;
    public bool CompanyIsActive { get; set; }
}

public class LoanProductFilterDto
{
    public string? Search { get; set; }
    public Guid? CompanyId { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinInterestRate { get; set; }
    public decimal? MaxInterestRate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? MinTenor { get; set; }
    public int? MaxTenor { get; set; }
    public InterestComputationBasis? InterestComputationBasis { get; set; }
    public InterestCostComputation? InterestCostComputation { get; set; }
    public PaymentScheduleBreakdown? PaymentScheduleBreakdown { get; set; }
    public PaymentScheduleType? PaymentScheduleType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedLoanProductListDto
{
    public List<LoanProductListDto> LoanProducts { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}