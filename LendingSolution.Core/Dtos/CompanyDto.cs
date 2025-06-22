namespace LendingSolution.Core.Dtos;

public class CompanyDto
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
}

public class CompanyResponseDto : CompanyDto
{
    public Guid Id { get; set; }
}

public class CreateCompanyRequestDto : CompanyDto;
