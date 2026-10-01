using Microsoft.AspNetCore.Authorization;

namespace LendingSolution.API.Auth;

/// <summary>
/// Requires the caller's token to carry the given permission claim.
/// Usage: [HasPermission(Permissions.Loans.Approve)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission)
    : AuthorizeAttribute(PermissionPolicyProvider.PolicyPrefix + permission);
