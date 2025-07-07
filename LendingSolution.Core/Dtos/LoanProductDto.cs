namespace LendingSolution.Core.Dtos;

public class LoanProductDto
{
    public required Guid CompanyId { get; set; }

    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    public int Moratorium { get; set; } = 0;

}

public class CreateLoanProductRequestDto : LoanProductDto;

public class UpdateLoanProductRequestDto
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    public int Moratorium { get; set; } = 0;
}

public class LoanProductResponseDto : LoanProductDto
{
    public Guid Id { get; set; }
}

public class LoanProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public int MinTenor { get; set; }
    public int MaxTenor { get; set; }
    public bool IsActive { get; set; }
    public int Moratorium { get; set; }
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