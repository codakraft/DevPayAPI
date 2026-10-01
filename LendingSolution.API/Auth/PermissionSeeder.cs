using System.Security.Claims;
using LendingSolution.Core.Auth;
using Microsoft.AspNetCore.Identity;

namespace LendingSolution.API.Auth;

public static class PermissionSeeder
{
    /// <summary>
    /// Makes each role's permission claims match <see cref="RolePermissions.Defaults"/>,
    /// adding missing ones and removing ones no longer in the mapping.
    /// </summary>
    public static async Task SyncRolePermissionsAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        foreach (var (roleName, permissions) in RolePermissions.Defaults)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                logger.LogWarning("Skipping permission sync for missing role {Role}", roleName);
                continue;
            }

            var existing = (await roleManager.GetClaimsAsync(role))
                .Where(c => c.Type == Permissions.ClaimType)
                .ToList();
            var existingValues = existing.Select(c => c.Value).ToHashSet();
            var desired = permissions.ToHashSet();

            foreach (var claim in existing.Where(c => !desired.Contains(c.Value)))
            {
                await roleManager.RemoveClaimAsync(role, claim);
                logger.LogInformation("Removed permission {Permission} from role {Role}", claim.Value, roleName);
            }

            foreach (var permission in desired.Where(p => !existingValues.Contains(p)))
            {
                await roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));
                logger.LogInformation("Added permission {Permission} to role {Role}", permission, roleName);
            }
        }
    }
}
