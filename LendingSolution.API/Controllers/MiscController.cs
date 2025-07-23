using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/misc")]
public class MiscController(IAuthService authService, ILogger<MiscController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<MiscController> _logger = logger;

    // Static bank list
    private static readonly List<BankInfo> Banks = new()
    {
        new BankInfo("000", "CENTRAL BANK OF NIGERIA"),
        new BankInfo("011", "FIRST BANK OF NIGERIA PLC"),
        new BankInfo("023", "NIGERIA INTERNATIONAL BANK (CITIBANK)"),
        new BankInfo("030", "BANK PLC"),
        new BankInfo("032", "UNION BANK OF NIGERIA PLC"),
        new BankInfo("033", "UBA PLC"),
        new BankInfo("035", "WEMA BANK PLC"),
        new BankInfo("039", "STANBIC IBTC BANK PLC"),
        new BankInfo("044", "ACCESS BANK PLC"),
        new BankInfo("050", "ECOBANK NIGERIA PLC"),
        new BankInfo("057", "ZENITH BANK PLC"),
        new BankInfo("058", "GUARANTY TRUST BANK"),
        new BankInfo("068", "STANDARD CHARTERED BANK NIGERIA LTD"),
        new BankInfo("070", "FIDELITY BANK PLC"),
        new BankInfo("076", "SKYE BANK PLC"),
        new BankInfo("101", "PROVIDUS BANK"),
        new BankInfo("214", "FIRST CITY MONUMENT BANK PLC"),
        new BankInfo("215", "UNITY BANK PLC"),
        new BankInfo("232", "STERLING BANK PLC"),
        new BankInfo("301", "JAIZ BANK"),
        new BankInfo("459", "CORONATION MERCHANT BANK LIMITED"),
        new BankInfo("480", "JUBILEE BANK"),
        new BankInfo("510120013", "KANO MICROFINANCE BANKS"),
        new BankInfo("511080016", "ESO SAVINGS LOANS PLC"),
        new BankInfo("511080026", "ASO SAVINGS AND LOANS"),
        new BankInfo("511080036", "NATIONAL HOUSING FUND"),
        new BankInfo("511080106", "GAA-AKANBI MICRO FINANCE BANK"),
        new BankInfo("512170012", "CLASSIC MICROFINANCE BANK"),
        new BankInfo("512170022", "SOLID ROCK MFB OKE ONA ABEOKUTA"),
        new BankInfo("512170032", "LAVENDER MICROFINANCE BANK LTD"),
        new BankInfo("512170042", "EBIGI MICROFINANCE BANK"),
        new BankInfo("512170052", "EFOTAMODI/OGUNOLA MICROFINANCE BANK"),
        new BankInfo("512170062", "EIYEPE MICROFINANCE BANK"),
        new BankInfo("512170072", "EJOSE MICROFINANCE BANK LTD"),
        new BankInfo("512170082", "EMAZING GRACE MICROFINANCE BANK"),
        new BankInfo("512170092", "CATLAND MICROFINANCE BANK LTD"),
        new BankInfo("512170102", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170112", "UKUOMBE MICROFINANCE BANK LTD"),
        new BankInfo("512170122", "URUWON MICROFINANCE BANK"),
        new BankInfo("512170132", "USO-E MICROFINANCE BANK"),
        new BankInfo("512170142", "EPPLE MICROFINANCE BANK"),
        new BankInfo("512170152", "OFONYIN MICROFINANCE BANK"),
        new BankInfo("512170162", "OMODI-IMOSAN MICROFINANCE BANK LTD"),
        new BankInfo("512170172", "OJEBU-IFE COMMUNITY BANK NIG. LTD"),
        new BankInfo("512170182", "OJEBU-IMUSIN MICROFINANCE BANK LTD"),
        new BankInfo("512170192", "OKENNE MICROFINANCE BANK LTD"),
        new BankInfo("512170202", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170212", "OLISAN MICROFINANCE BANK LTD"),
        new BankInfo("512170222", "OMOWO MICROFINANCE BANK NIG LTD"),
        new BankInfo("512170232", "ONTERLAND MICROFINANCE BANK"),
        new BankInfo("512170242", "OPERU MICROFINANCE BANK LTD"),
        new BankInfo("512170252", "OTELE MICROFINANCE BANK LTD"),
        new BankInfo("512170262", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170272", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170282", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170292", "MALPOLY MICROFINANCE BANK"),
        new BankInfo("512170302", "MOLUSI MICROFINANCE BANK LTD"),
        new BankInfo("512170312", "NEW IMAGE MFB EGBA ODEDA"),
        new BankInfo("512170322", "COMBINED BENEFITS MICROFINANCE BANK"),
        new BankInfo("512170332", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170342", "AMU COMMUNITY BANK LTD"),
        new BankInfo("512170352", "ARISUN MFB"),
        new BankInfo("512170362", "RIVERSIDE MICROFINANCE BANK LTD"),
        new BankInfo("512170372", "SAGAM MICROFINANCE BANK"),
        new BankInfo("512170382", "TRUST MFB EGBA OWODE ABEOKUTA"),
        new BankInfo("512170392", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170402", "ACON SUCCESS MICROFINANCE BANK LTD"),
        new BankInfo("512170412", "UNAAB MICROFINANCE BANK"),
        new BankInfo("512170422", "WEST-END MICROFINANCE BANK"),
        new BankInfo("512170432", "HONEY MICROFINANCE BANK"),
        new BankInfo("512170442", "EGOSASA MICROFINANCE BANK"),
        new BankInfo("512170452", "ESTRA POLARIS MICROFINANCE BANK LTD"),
        new BankInfo("512170462", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170472", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170492", "STAR MICROFINANCE BANK"),
        new BankInfo("512170502", "CENTAGE SAVINGS AND LOANS"),
        new BankInfo("512170522", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170542", "GATEWAY SAVINGS AND LOANS LTD"),
        new BankInfo("512170552", "NACRDB ABIGI"),
        new BankInfo("512170562", "NACRDB AYETORO"),
        new BankInfo("512170572", "NACRDB IMEKO"),
        new BankInfo("512170582", "NACRDB ABEOKUTA"),
        new BankInfo("512170592", "OGUN STATE MICROFINANCE BANKS"),
        new BankInfo("512170602", "NACRDB OTA"),
        new BankInfo("512170612", "NACRDB ODEDA ABEOKUTA"),
        new BankInfo("512170622", "NACRDB AGO-IWOYE"),
        new BankInfo("512170632", "NACRDB SAGAM"),
        new BankInfo("512170652", "ONTEGRATED MICROFINANCE BANK"),
        new BankInfo("512170662", "OROLU MIRCOFINANCE BANK LTD"),
        new BankInfo("513210013", "RIVERS MICROFINANCE BANKS"),
        new BankInfo("514040013", "EDO MICROFINANCE BANKS"),
        new BankInfo("515150013", "LAGOS BUILDING INVESTMENT CO.LTD"),
        new BankInfo("515150023", "SKYE MORTGAGE"),
        new BankInfo("515150033", "UNION HOMES SAVINGS AND LOANS PLC"),
        new BankInfo("515150043", "NIGERIA POLICE FORCE"),
        new BankInfo("515150053", "NIGERIA POLICE FORCE"),
        new BankInfo("515150063", "LAGOS MICROFINANCE BANKS"),
        new BankInfo("515150073", "SPECSMFB"),
        new BankInfo("516290013", "OSUN MICROFINANCE BANKS"),
        new BankInfo("516290023", "AAU MICRO-FINANCE BANK"),
        new BankInfo("517150014", "ARITA BASHORUN MICROFINANCE BANK"),
        new BankInfo("517150024", "MULTIVEST MICROFINANCE BANK"),
        new BankInfo("517150034", "OGBOORA MFB"),
        new BankInfo("517150044", "PACESETTERS MICROFINANCE BANK"),
        new BankInfo("517150054", "CIVIC MICROFINANCE BANK"),
        new BankInfo("517150064", "ALOGBON MFB"),
        new BankInfo("517150074", "CREST MFB"),
        new BankInfo("517150084", "EYETE MFB"),
        new BankInfo("517150094", "EKESAN MFB"),
        new BankInfo("518150014", "YOBE SAVINGS & LOANS LIMITED"),
        new BankInfo("518150024", "NIG AGRIC COOP & RURAL DEV BANK LTD"),
        new BankInfo("518150034", "YOBE MICROFINANCE BANK"),
        new BankInfo("518150044", "YOBE MICROFINANCE BANK"),
        new BankInfo("518150054", "YOBE MICROFINANCE BANK"),
        new BankInfo("518150064", "YOBE MICROFINANCE BANK"),
        new BankInfo("519290019", "ZION MFB"),
        new BankInfo("520150010", "AURS MICRO FINANCE BANK"),
        new BankInfo("520150020", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150030", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150040", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150050", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150060", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150070", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150080", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150090", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150100", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150110", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150120", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150130", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150140", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150150", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150160", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150170", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("520150180", "KWARA STATE MICROFINANCE BANKS"),
        new BankInfo("521150010", "OKOLE MICROFINANCE BANK"),
        new BankInfo("521150020", "SUNBEAM MICROFINANCE BANK LTD"),
        new BankInfo("521150030", "ERAMOKO MFB LIMITED"),
        new BankInfo("522150010", "JIGAWA SAVINGS & LOANS LTD"),
        new BankInfo("580000010", "JUBILEE LIFE MORTGAGE BANK"),
        new BankInfo("590000001", "POCKET MONEY")
    };

    public record BankInfo(string BankCode, string BankName);

    [HttpGet("banks")]
    public IActionResult GetBanks([FromQuery] string? bankCode = null, [FromQuery] string? bankName = null)
    {
        var query = Banks.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(bankCode))
            query = query.Where(b => b.BankCode.Contains(bankCode, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(bankName))
            query = query.Where(b => b.BankName.Contains(bankName, StringComparison.OrdinalIgnoreCase));
        return Ok(ApiResponse.Ok("banks fetched successfully", query.ToList()));
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _authService.GetRoles();
            return Ok(ApiResponse.Ok("roles fetched successfully", roles));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching roles.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}