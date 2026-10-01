using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LendingSolution.API.Filters;

/// <summary>
/// Marks an endpoint as reachable by a user whose token still carries the
/// password-change-required claim (e.g. change-password, logout, refresh).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowWhilePasswordChangeRequiredAttribute : Attribute;

/// <summary>
/// Blocks every other endpoint for users who must change their password first.
/// </summary>
public class PasswordChangeRequiredFilter : IAuthorizationFilter
{
    public const string ClaimType = "pwd_change_required";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true || !user.HasClaim(ClaimType, "true"))
        {
            return;
        }

        var allowed = context.ActionDescriptor.EndpointMetadata
            .OfType<AllowWhilePasswordChangeRequiredAttribute>()
            .Any();

        if (!allowed)
        {
            context.Result = new ObjectResult(
                ApiResponse.Fail("You must change your password before continuing").WithCode(ErrorCodes.PasswordChangeRequired))
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
