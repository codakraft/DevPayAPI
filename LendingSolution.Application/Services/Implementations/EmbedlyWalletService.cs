using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;

namespace LendingSolution.Application.Services.Implementations;

public class EmbedlyWalletService : IEmbedlyWalletService
{
    private readonly EmbedlySettings _settings;
    private readonly HttpClient _httpClient;
    private readonly IEWalletRepository _eWalletRepository;
    private readonly IEWalletTransactionRepository _eWalletTransactionRepository;
    private readonly ILogger<EmbedlyWalletService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EmbedlyWalletService(
        IOptions<EmbedlySettings> options,
        IHttpClientFactory httpClientFactory,
        IEWalletRepository eWalletRepository,
        IEWalletTransactionRepository eWalletTransactionRepository,
        ILogger<EmbedlyWalletService> logger)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient("EmbedlyWalletClient");
        _eWalletRepository = eWalletRepository;
        _eWalletTransactionRepository = eWalletTransactionRepository;
        _logger = logger;

        _httpClient.DefaultRequestHeaders.Add("x-api-key", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        _logger.LogInformation("EmbedlyWalletService initialized - BaseUrl={BaseUrl}", _settings.BaseUrl);
    }

    // ─── Customer ─────────────────────────────────────────────────────────────

    public async Task<EmbedlyCreateCustomerResponseDto?> CreateCustomerAsync(EmbedlyCreateCustomerRequestDto request)
    {
        var url = $"{_settings.BaseUrl}/customers/add";
        try
        {
            _logger.LogInformation("Embedly: creating customer Email={Email}", request.EmailAddress);

            var existing = await _eWalletRepository.GetByMobileNumberAsync(request.MobileNumber);
            if (existing != null)
            {
                _logger.LogInformation("Embedly: customer already exists MobileNumber={MobileNumber}", request.MobileNumber);
                return new EmbedlyCreateCustomerResponseDto
                {
                    Status  = true,
                    Message = "Customer already exists",
                    Data    = new EmbedlyCustomerDto
                    {
                        CustomerId   = existing.EmbedlyCustomerId,
                        FirstName    = existing.FirstName,
                        LastName     = existing.LastName,
                        MiddleName   = existing.MiddleName,
                        Email        = existing.EmailAddress,
                        MobileNumber = existing.MobileNumber,
                        DateOfBirth  = existing.Dob,
                        Address      = existing.Address,
                        City         = existing.City,
                        KycTier      = existing.KycTier
                    }
                };
            }

            var payload = new
            {
                organizationId = _settings.OrganizationId,
                firstName      = request.FirstName,
                lastName       = request.LastName,
                middleName     = request.MiddleName,
                emailAddress   = request.EmailAddress,
                mobileNumber   = request.MobileNumber,
                dob            = request.Dob,
                customerTypeId = _settings.CustomerTypeId,
                address        = request.Address,
                city           = request.City,
                countryId      = _settings.CountryId,
                alias          = request.Alias
            };

            var httpResponse = await _httpClient.PostAsync(url, Serialize(payload));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly create customer response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly: {Url} failed Status={Status}", url, (int)httpResponse.StatusCode);
                return default;
            }

            var result = JsonSerializer.Deserialize<EmbedlyCreateCustomerResponseDto>(body, _jsonOptions);

            _logger.LogInformation("Embedly create customer result - Status={Status} CustomerId={CustomerId}",
                result?.Status, result?.Data?.CustomerId);

            if (result?.Status == true && result.Data != null)
            {
                var record = new EWallet
                {
                    EmbedlyCustomerId   = result.Data.CustomerId,
                    FirstName           = result.Data.FirstName,
                    LastName            = result.Data.LastName,
                    MiddleName          = result.Data.MiddleName,
                    EmailAddress        = result.Data.Email,
                    MobileNumber        = result.Data.MobileNumber,
                    Dob                 = result.Data.DateOfBirth,
                    Address             = result.Data.Address,
                    City                = result.Data.City,
                    KycTier             = result.Data.KycTier,
                    Alias               = request.Alias,
                    CustomerRawResponse = body
                };

                await _eWalletRepository.CreateAsync(record);
                _logger.LogInformation("Embedly: customer saved to EWallets DB CustomerId={CustomerId}",
                    result.Data.CustomerId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error creating customer");
            return null;
        }
    }

    public async Task<EmbedlyCreateCustomerV2ResponseDto?> CreateCustomerV2Async(EmbedlyCreateCustomerV2RequestDto request)
    {
        var url = $"{_settings.WaasCoreV2BaseUrl}/customers/add";
        try
        {
            _logger.LogInformation("Embedly v2: creating customer Email={Email} MobileNumber={MobileNumber}",
                request.EmailAddress, request.MobileNumber);

            var existing = await _eWalletRepository.GetByMobileNumberAsync(request.MobileNumber);
            if (existing != null)
            {
                _logger.LogInformation("Embedly v2: customer already exists MobileNumber={MobileNumber}", request.MobileNumber);
                return new EmbedlyCreateCustomerV2ResponseDto
                {
                    StatusCode = 200,
                    Message    = "Customer already exists",
                    Data    = new EmbedlyCustomerV2Dto
                    {
                        CustomerId       = existing.EmbedlyCustomerId,
                        FirstName        = existing.FirstName,
                        LastName         = existing.LastName,
                        MiddleName       = existing.MiddleName,
                        Email            = existing.EmailAddress,
                        MobileNumber     = existing.MobileNumber,
                        DateOfBirth      = existing.Dob,
                        Address          = existing.Address,
                        City             = existing.City,
                        Alias            = existing.Alias,
                        KycTier          = existing.KycTier,
                        Gender           = existing.Gender,
                        MaritalStatus    = existing.MaritalStatus,
                        Occupation       = existing.Occupation,
                        PassportUrl      = existing.PassportUrl,
                        MothersMaidenName = existing.MothersMaidenName,
                        NextOfKin = existing.NextOfKinSurname != null ? new EmbedlyNextOfKinDto
                        {
                            Surname      = existing.NextOfKinSurname,
                            FirstName    = existing.NextOfKinFirstName,
                            OtherNames   = existing.NextOfKinOtherNames,
                            Relationship = existing.NextOfKinRelationship,
                            MobileNumber = existing.NextOfKinMobileNumber,
                            Address      = existing.NextOfKinAddress
                        } : null
                    }
                };
            }

            var payload = new
            {
                firstName        = request.FirstName,
                lastName         = request.LastName,
                middleName       = request.MiddleName,
                dob              = request.Dob,
                customerTypeId   = _settings.CustomerTypeId,
                alias            = request.Alias,
                countryId        = _settings.CountryId,
                city             = request.City,
                address          = request.Address,
                mobileNumber     = request.MobileNumber,
                emailAddress     = request.EmailAddress,
                gender           = request.Gender,
                maritalStatus    = request.MaritalStatus,
                pepDeclaration   = _settings.PepDeclaration,
                employmentStatus = _settings.EmploymentStatus,
                customerTierId   = _settings.CustomerTierId,
                occupation       = request.Occupation,
                sourceOfFunds    = _settings.SourceOfFunds,
                passportUrl      = request.PassportUrl,
                mothersMaidenName = request.MothersMaidenName,
                nextOfKin        = new
                {
                    surname      = request.NextOfKin.Surname,
                    firstName    = request.NextOfKin.FirstName,
                    otherNames   = request.NextOfKin.OtherNames,
                    relationship = request.NextOfKin.Relationship,
                    mobileNumber = request.NextOfKin.MobileNumber,
                    address      = request.NextOfKin.Address
                }
            };

            var requestJson = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Embedly v2 create customer request - Url={Url} Body={Body}", url, requestJson);

            var httpResponse = await _httpClient.PostAsync(url, Serialize(payload));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly v2 create customer response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly v2: {Url} failed Status={Status}", url, (int)httpResponse.StatusCode);
                return default;
            }

            var result = JsonSerializer.Deserialize<EmbedlyCreateCustomerV2ResponseDto>(body, _jsonOptions);

            _logger.LogInformation("Embedly v2 create customer result - Status={Status} CustomerId={CustomerId}",
                result?.StatusCode, result?.Data?.CustomerId);

            if (result?.Data != null)
            {
                var record = new EWallet
                {
                    EmbedlyCustomerId  = result.Data.CustomerId,
                    FirstName          = result.Data.FirstName ?? request.FirstName,
                    LastName           = result.Data.LastName ?? request.LastName,
                    MiddleName         = result.Data.MiddleName,
                    EmailAddress       = result.Data.Email,
                    MobileNumber       = result.Data.MobileNumber,
                    Dob                = result.Data.DateOfBirth,
                    Address            = result.Data.Address,
                    City               = result.Data.City,
                    Alias              = result.Data.Alias,
                    KycTier            = result.Data.KycTier,
                    Gender             = result.Data.Gender,
                    MaritalStatus      = result.Data.MaritalStatus,
                    Occupation         = result.Data.Occupation,
                    PassportUrl        = result.Data.PassportUrl,
                    MothersMaidenName  = result.Data.MothersMaidenName,
                    NextOfKinSurname      = result.Data.NextOfKin?.Surname,
                    NextOfKinFirstName    = result.Data.NextOfKin?.FirstName,
                    NextOfKinOtherNames   = result.Data.NextOfKin?.OtherNames,
                    NextOfKinRelationship = result.Data.NextOfKin?.Relationship,
                    NextOfKinMobileNumber = result.Data.NextOfKin?.MobileNumber,
                    NextOfKinAddress      = result.Data.NextOfKin?.Address,
                    CustomerRawResponse   = body
                };

                await _eWalletRepository.CreateAsync(record);
                _logger.LogInformation("Embedly v2: customer saved to EWallets DB CustomerId={CustomerId}",
                    result.Data.CustomerId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly v2: error creating customer");
            return null;
        }
    }

    public async Task<EmbedlyUpdateCustomerV2ResponseDto?> UpdateCustomerV2Async(string customerId, EmbedlyUpdateCustomerV2RequestDto request)
    {
        var url = $"{_settings.WaasCoreV2BaseUrl}/customers/customer/{customerId}/update";
        try
        {
            var payload = new
            {
                organizationId   = _settings.OrganizationId,
                firstName        = request.FirstName,
                lastName         = request.LastName,
                middleName       = request.MiddleName,
                dob              = request.Dob,
                city             = request.City,
                address          = request.Address,
                occupation       = request.Occupation,
                gender           = request.Gender,
                bvnverified      = request.BvnVerified,
                ninVerified      = request.NinVerified,
                bvn              = request.Bvn,
                nin              = request.Nin,
                maritalStatus    = request.MaritalStatus,
                pepDeclaration   = _settings.PepDeclaration,
                employmentStatus = _settings.EmploymentStatus,
                sourceOfFunds    = _settings.SourceOfFunds,
                passportUrl      = request.PassportUrl,
                nextOfKin        = request.NextOfKin == null ? null : new
                {
                    surname      = request.NextOfKin.Surname,
                    firstName    = request.NextOfKin.FirstName,
                    otherNames   = request.NextOfKin.OtherNames,
                    relationship = request.NextOfKin.Relationship,
                    mobileNumber = request.NextOfKin.MobileNumber,
                    address      = request.NextOfKin.Address
                }
            };

            var requestJson = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Embedly v2 update customer request - Url={Url} Body={Body}", url, requestJson);

            var httpResponse = await _httpClient.PatchAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly v2 update customer response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly v2: {Url} failed Status={Status}", url, (int)httpResponse.StatusCode);
                return default;
            }

            var result = JsonSerializer.Deserialize<EmbedlyUpdateCustomerV2ResponseDto>(body, _jsonOptions);

            _logger.LogInformation("Embedly v2 update customer result - Status={Status} CustomerId={CustomerId}",
                result?.StatusCode, result?.Data?.CustomerId);

            if (result?.Data != null)
            {
                var record = await _eWalletRepository.GetByEmbedlyCustomerIdAsync(customerId);
                if (record != null)
                {
                    record.FirstName          = result.Data.FirstName  ?? record.FirstName;
                    record.LastName           = result.Data.LastName   ?? record.LastName;
                    record.MiddleName         = result.Data.MiddleName ?? record.MiddleName;
                    record.Dob                = result.Data.DateOfBirth ?? record.Dob;
                    record.Address            = result.Data.Address    ?? record.Address;
                    record.City               = result.Data.City       ?? record.City;
                    record.Gender             = result.Data.Gender     ?? record.Gender;
                    record.MaritalStatus      = result.Data.MaritalStatus ?? record.MaritalStatus;
                    record.Occupation         = result.Data.Occupation ?? record.Occupation;
                    record.PassportUrl        = result.Data.PassportUrl ?? record.PassportUrl;
                    record.Bvn                = request.Bvn            ?? record.Bvn;
                    record.BvnVerified        = request.BvnVerified    ?? record.BvnVerified;
                    record.Nin                = request.Nin            ?? record.Nin;
                    record.NinVerified        = request.NinVerified    ?? record.NinVerified;
                    record.NextOfKinSurname      = result.Data.NextOfKin?.Surname      ?? record.NextOfKinSurname;
                    record.NextOfKinFirstName    = result.Data.NextOfKin?.FirstName    ?? record.NextOfKinFirstName;
                    record.NextOfKinOtherNames   = result.Data.NextOfKin?.OtherNames   ?? record.NextOfKinOtherNames;
                    record.NextOfKinRelationship = result.Data.NextOfKin?.Relationship ?? record.NextOfKinRelationship;
                    record.NextOfKinMobileNumber = result.Data.NextOfKin?.MobileNumber ?? record.NextOfKinMobileNumber;
                    record.NextOfKinAddress      = result.Data.NextOfKin?.Address      ?? record.NextOfKinAddress;
                    record.CustomerRawResponse   = body;

                    await _eWalletRepository.UpdateAsync(record);
                    _logger.LogInformation("Embedly v2: EWallets record updated for CustomerId={CustomerId}", customerId);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly v2: error updating customer CustomerId={CustomerId}", customerId);
            return null;
        }
    }

    public async Task<EmbedlyGetCustomerResponseDto?> GetCustomerByIdAsync(string customerId)
    {
        var url = $"{_settings.BaseUrl}/customers/get/id/{customerId}";
        try
        {
            _logger.LogInformation("Embedly: fetching customer CustomerId={CustomerId}", customerId);

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetCustomerResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching customer CustomerId={CustomerId}", customerId);
            return null;
        }
    }

    public async Task<EmbedlyGetAllCustomersResponseDto?> GetAllCustomersAsync()
    {
        var url = $"{_settings.BaseUrl}/customers/get/all";
        try
        {
            _logger.LogInformation("Embedly: fetching all customers");

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetAllCustomersResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching all customers");
            return null;
        }
    }

    // ─── Wallet ───────────────────────────────────────────────────────────────

    public async Task<EmbedlyCreateWalletResponseDto?> CreateWalletAsync(EmbedlyCreateWalletRequestDto request)
    {
        var url = $"{_settings.BaseUrl}/wallets/add";
        try
        {
            var payload = new
            {
                customerId = request.CustomerId,
                currencyId = _settings.CurrencyId,
                name       = request.Name
            };

            var requestJson = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Embedly create wallet request - Url={Url} Body={Body}", url, requestJson);

            var httpResponse = await _httpClient.PostAsync(url, Serialize(payload));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly create wallet response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly: {Url} failed Status={Status}", url, (int)httpResponse.StatusCode);
                return default;
            }

            var result = JsonSerializer.Deserialize<EmbedlyCreateWalletResponseDto>(body, _jsonOptions);

            if (result?.Status == true && result.Data != null)
            {
                // Update the existing EWallet record created during CreateCustomer
                var record = await _eWalletRepository.GetByEmbedlyCustomerIdAsync(request.CustomerId);

                var virtualAccount = result.Data.VirtualAccount;

                if (record != null)
                {
                    record.EmbedlyWalletId   = result.Data.WalletId;
                    record.WalletGroupId     = result.Data.WalletGroupId;
                    record.CurrencyId        = result.Data.CurrencyId;
                    record.AccountNumber     = virtualAccount?.AccountNumber;
                    record.BankCode          = virtualAccount?.BankCode;
                    record.BankName          = virtualAccount?.BankName;
                    record.AccountName       = result.Data.Name;
                    record.AvailableBalance  = result.Data.AvailableBalance;
                    record.LedgerBalance     = result.Data.LedgerBalance;
                    record.IsDefault         = result.Data.IsDefault;
                    record.Classification    = result.Data.Classification;
                    record.WalletRawResponse = body;

                    await _eWalletRepository.UpdateAsync(record);
                    _logger.LogInformation("Embedly: EWallets record updated WalletId={WalletId} AccountNumber={AccountNumber}",
                        result.Data.WalletId, virtualAccount?.AccountNumber);
                }
                else
                {
                    // Customer wasn't created via our API — create a fresh record with wallet data only
                    var freshRecord = new EWallet
                    {
                        EmbedlyCustomerId = result.Data.CustomerId ?? request.CustomerId,
                        FirstName         = result.Data.Name ?? string.Empty,
                        LastName          = string.Empty,
                        EmbedlyWalletId   = result.Data.WalletId,
                        WalletGroupId     = result.Data.WalletGroupId,
                        CurrencyId        = result.Data.CurrencyId,
                        AccountNumber     = virtualAccount?.AccountNumber,
                        BankCode          = virtualAccount?.BankCode,
                        BankName          = virtualAccount?.BankName,
                        AccountName       = result.Data.Name,
                        AvailableBalance  = result.Data.AvailableBalance,
                        LedgerBalance     = result.Data.LedgerBalance,
                        IsDefault         = result.Data.IsDefault,
                        Classification    = result.Data.Classification,
                        WalletRawResponse = body
                    };

                    await _eWalletRepository.CreateAsync(freshRecord);
                    _logger.LogInformation("Embedly: new EWallets record created for WalletId={WalletId}",
                        result.Data.WalletId);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error creating wallet for CustomerId={CustomerId}", request.CustomerId);
            return null;
        }
    }

    public async Task<EmbedlyGetWalletResponseDto?> GetWalletByIdAsync(string walletId)
    {
        var url = $"{_settings.BaseUrl}/wallets/get/wallet/{walletId}";
        try
        {
            _logger.LogInformation("Embedly: fetching wallet WalletId={WalletId}", walletId);

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetWalletResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching wallet WalletId={WalletId}", walletId);
            return null;
        }
    }

    public async Task<EmbedlyGetWalletResponseDto?> GetWalletByAccountNumberAsync(string accountNumber)
    {
        var url = $"{_settings.BaseUrl}/wallets/get/wallet/account/{accountNumber}";
        try
        {
            _logger.LogInformation("Embedly: fetching wallet AccountNumber={AccountNumber}", accountNumber);

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetWalletResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching wallet AccountNumber={AccountNumber}", accountNumber);
            return null;
        }
    }

    public async Task<EmbedlyGetWalletsByCustomerResponseDto?> GetWalletsByCustomerIdAsync(string customerId)
    {
        var url = $"{_settings.WaasCoreBaseUrl}/wallets/get/list/{customerId}";
        try
        {
            _logger.LogInformation("Embedly: fetching wallets for CustomerId={CustomerId}", customerId);

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetWalletsByCustomerResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching wallets for CustomerId={CustomerId}", customerId);
            return null;
        }
    }

    public async Task<EmbedlyWalletTransferResponseDto?> WalletTransferAsync(EmbedlyWalletTransferRequestDto request)
    {
        var url = $"{_settings.BaseUrl}/wallets/wallet/transaction/v2/wallet-to-wallet";
        var transactionReference = $"EWT-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        try
        {
            var payload = new
            {
                fromAccount          = request.FromAccount,
                toAccount            = request.ToAccount,
                amount               = request.Amount,
                transactionReference = transactionReference,
                remarks              = request.Remarks
            };

            var requestJson = JsonSerializer.Serialize(payload);
            _logger.LogInformation("Embedly wallet transfer request - From={From} To={To} Amount={Amount} Ref={Ref}",
                request.FromAccount, request.ToAccount, request.Amount, transactionReference);

            var httpResponse = await _httpClient.PutAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly wallet transfer response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            var succeeded = httpResponse.IsSuccessStatusCode;

            if (!succeeded)
                _logger.LogError("Embedly: wallet transfer failed Status={Status} From={From} To={To}",
                    (int)httpResponse.StatusCode, request.FromAccount, request.ToAccount);

            var result = succeeded
                ? JsonSerializer.Deserialize<EmbedlyWalletTransferResponseDto>(body, _jsonOptions)
                : default;

            var transferSucceeded = succeeded && result?.Success == true;

            await _eWalletTransactionRepository.CreateAsync(new EWalletTransaction
            {
                TransactionReference = transactionReference,
                FromAccount          = request.FromAccount,
                ToAccount            = request.ToAccount,
                Amount               = request.Amount,
                Remarks              = request.Remarks,
                Status               = transferSucceeded ? "Success" : "Failed",
                RawResponse          = body
            });

            if (transferSucceeded)
                await UpdateWalletBalancesAsync(request.FromAccount, request.ToAccount, request.Amount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error processing wallet transfer From={From} To={To} Ref={Ref}",
                request.FromAccount, request.ToAccount, transactionReference);
            return null;
        }
    }

    public async Task<EmbedlyFundAccountResponseDto?> FundAccountAsync(EmbedlyFundAccountRequestDto request)
    {
        var url = $"{_settings.WaasCoreBaseUrl}/nip/inflow/simulate-inflow";
        try
        {
            var requestJson = JsonSerializer.Serialize(request);
            _logger.LogInformation("Embedly fund account request - Url={Url} AccountNumber={AccountNumber} Amount={Amount}",
                url, request.BeneficiaryAccountNumber, request.Amount);

            var httpResponse = await _httpClient.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly fund account response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly: fund account failed Status={Status} AccountNumber={AccountNumber}",
                    (int)httpResponse.StatusCode, request.BeneficiaryAccountNumber);
                return default;
            }

            var result = JsonSerializer.Deserialize<EmbedlyFundAccountResponseDto>(body, _jsonOptions);

            if (result?.Success == true && decimal.TryParse(request.Amount, out var amount))
                await CreditWalletBalanceAsync(request.BeneficiaryAccountNumber, amount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error funding account AccountNumber={AccountNumber}", request.BeneficiaryAccountNumber);
            return null;
        }
    }

    public async Task<EmbedlyKycUpgradeResponseDto?> NinKycUpgradeAsync(string customerId, string nin, EmbedlyNinKycUpgradeRequestDto request)
    {
        var url = $"{_settings.BaseUrl}/customers/kyc/customer/nin?customerId={Uri.EscapeDataString(customerId)}&nin={Uri.EscapeDataString(nin)}&verify=1";
        try
        {
            var requestJson = JsonSerializer.Serialize(request);
            _logger.LogInformation("Embedly NIN KYC upgrade request - CustomerId={CustomerId} Url={Url}", customerId, url);

            var httpResponse = await _httpClient.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly NIN KYC upgrade response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly: NIN KYC upgrade failed Status={Status} CustomerId={CustomerId}", (int)httpResponse.StatusCode, customerId);
                return default;
            }

            return JsonSerializer.Deserialize<EmbedlyKycUpgradeResponseDto>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error during NIN KYC upgrade CustomerId={CustomerId}", customerId);
            return null;
        }
    }

    public async Task<EmbedlyKycUpgradeResponseDto?> BvnKycUpgradeAsync(EmbedlyBvnKycUpgradeRequestDto request)
    {
        var url = $"{_settings.BaseUrl}/customers/kyc/premium-kyc?verify=1";
        try
        {
            var requestJson = JsonSerializer.Serialize(request);
            _logger.LogInformation("Embedly BVN KYC upgrade request - CustomerId={CustomerId} Url={Url}", request.CustomerId, url);

            var httpResponse = await _httpClient.PostAsync(url, new StringContent(requestJson, Encoding.UTF8, "application/json"));
            var body = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogInformation("Embedly BVN KYC upgrade response - Status={Status} Body={Body}",
                (int)httpResponse.StatusCode, body);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Embedly: BVN KYC upgrade failed Status={Status} CustomerId={CustomerId}", (int)httpResponse.StatusCode, request.CustomerId);
                return default;
            }

            return JsonSerializer.Deserialize<EmbedlyKycUpgradeResponseDto>(body, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error during BVN KYC upgrade CustomerId={CustomerId}", request.CustomerId);
            return null;
        }
    }

    public async Task<EmbedlyGetKycStatusResponseDto?> GetKycStatusAsync(string customerId)
    {
        var url = $"{_settings.BaseUrl}/customers/customer-verification-properties/{customerId}";
        try
        {
            _logger.LogInformation("Embedly: fetching KYC status CustomerId={CustomerId}", customerId);

            var response = await _httpClient.GetAsync(url);
            return await ReadResponse<EmbedlyGetKycStatusResponseDto>(response, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedly: error fetching KYC status CustomerId={CustomerId}", customerId);
            return null;
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task CreditWalletBalanceAsync(string accountNumber, decimal amount)
    {
        var wallet = await _eWalletRepository.GetByAccountNumberAsync(accountNumber);
        if (wallet == null)
        {
            _logger.LogWarning("EWallet balance update skipped — account not found AccountNumber={AccountNumber}", accountNumber);
            return;
        }

        wallet.AvailableBalance = (wallet.AvailableBalance ?? 0) + amount;
        wallet.LedgerBalance    = (wallet.LedgerBalance    ?? 0) + amount;
        await _eWalletRepository.UpdateAsync(wallet);
        _logger.LogInformation("EWallet credited AccountNumber={AccountNumber} Amount={Amount} NewBalance={Balance}",
            accountNumber, amount, wallet.AvailableBalance);
    }

    private async Task DebitWalletBalanceAsync(string accountNumber, decimal amount)
    {
        var wallet = await _eWalletRepository.GetByAccountNumberAsync(accountNumber);
        if (wallet == null)
        {
            _logger.LogWarning("EWallet balance update skipped — account not found AccountNumber={AccountNumber}", accountNumber);
            return;
        }

        wallet.AvailableBalance = (wallet.AvailableBalance ?? 0) - amount;
        wallet.LedgerBalance    = (wallet.LedgerBalance    ?? 0) - amount;
        await _eWalletRepository.UpdateAsync(wallet);
        _logger.LogInformation("EWallet debited AccountNumber={AccountNumber} Amount={Amount} NewBalance={Balance}",
            accountNumber, amount, wallet.AvailableBalance);
    }

    private async Task UpdateWalletBalancesAsync(string fromAccount, string toAccount, decimal amount)
    {
        await DebitWalletBalanceAsync(fromAccount, amount);
        await CreditWalletBalanceAsync(toAccount, amount);
    }

    private static StringContent Serialize<T>(T payload) =>
        new(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

    private async Task<T?> ReadResponse<T>(HttpResponseMessage response, string url)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var result = JsonSerializer.Deserialize<T>(body, _jsonOptions);
            _logger.LogInformation("Embedly: {Url} succeeded Body={Body}", url, body);
            return result;
        }

        _logger.LogError("Embedly: {Url} failed Status={Status} Body={Body}",
            url, (int)response.StatusCode, body);
        return default;
    }
}
