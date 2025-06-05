using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models.Response;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRepaymentService
{
    Task<ApiResponse> MakeRepayment(RepaymentDto dto, ClaimsPrincipal user);
}