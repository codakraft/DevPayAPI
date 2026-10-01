namespace LendingSolution.Core.Dtos;

public class RoleAssignDto
{
    public required string UserId { get; set; }
    public required string RoleId { get; set; }
}

public class RoleSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class RoleWithPermissionsDto : RoleSummaryDto
{
    public List<string> Permissions { get; set; } = [];
}
