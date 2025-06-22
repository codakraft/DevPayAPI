namespace LendingSolution.Core.Dtos;

public class CreateAdminRequestDto : CreateSuperAdminDto
{
    public Guid CompanyId { get; set; }
}