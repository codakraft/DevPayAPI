using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize(Roles = "FinanceOfficer")]
public class FiannceController : Controller
{
    // [GET]    /api/finance/disbursements  
    // [GET]    /api/finance/repayments  
    // [POST]   /api/finance/disbursements/{loanId}  
    // [POST]   /api/finance/repayments  
    // [GET]    /api/finance/reports/monthly  
    // [GET]    /api/finance/reports/company/{companyId}  
    // [GET]    /api/finance/wallet/{companyId}  

}

