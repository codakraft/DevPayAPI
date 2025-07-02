using LendingSolution.Application.Services.Implementations;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
public class UserController(
    IAuthService authService,
    ILoanService loanService,
    ILoanProductService loanProductService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanService _loanService = loanService;
    private readonly ILoanProductService _loanProductService = loanProductService;

}
