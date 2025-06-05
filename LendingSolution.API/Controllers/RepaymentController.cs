using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepaymentController(IRepaymentService repaymentService) : Controller
{
    private readonly IRepaymentService _repaymentService = repaymentService;

    [HttpPost("pay")]
    public async Task<IActionResult> Pay([FromBody] RepaymentDto body)
    {
        var result = await _repaymentService.MakeRepayment(body, User);
        return Ok(result);
    }
}