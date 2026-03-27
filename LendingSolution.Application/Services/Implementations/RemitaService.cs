using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Helpers;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Enum;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

public class RemitaService : IRemitaService
{
    private readonly RemitaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemitaService> _logger;
    private readonly ICombinedRepository _cRepo;
    private readonly ApplicationDbContext _db;

    public RemitaService(
        IOptions<RemitaSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<RemitaService> logger,
        ICombinedRepository cRepo,
        ApplicationDbContext db)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient("RemitaClient");
        _logger = logger;
        _cRepo = cRepo;
        _db = db;

        _logger.LogInformation("RemitaService initialized - BaseUrl={BaseUrl}, UseLiveData={UseLiveData}",
            _settings.BaseUrl, _settings.UseLiveData);
    }

    // Salary History Method
    public async Task<RemitaSalaryHistoryResponseDto?> GetSalaryHistoryAsync(
        string accountNumber,
        string bankCode,
        string bvn,
        Guid borrowerApplicationId,
        string firstName = "",
        string lastName = "",
        string middleName = "",
        string? authorisationCode = null
    )
    {
        try
        {
            // Check if we should use test data for non-live mode
            string finalAuthCode;

            if (!_settings.UseLiveData)
            {
                _logger.LogInformation("UseLiveData is false - using Remita test data for salary history");
                _logger.LogInformation("Original values - Account: {AccountNumber}, Bank: {BankCode}, BVN: {BVN}",
                    accountNumber, bankCode, bvn);
                
                // Override with Remita test account details
                accountNumber = "5012284010";
                bankCode = "023";
                bvn = "22222222222";
                
                _logger.LogInformation("Test values applied - Account: {AccountNumber}, Bank: {BankCode}, BVN: {BVN}",
                    accountNumber, bankCode, bvn);
            }
            else
            {
                _logger.LogInformation("UseLiveData is true - using borrower data - Account: {AccountNumber}, Bank: {BankCode}",
                    accountNumber, bankCode);
            }
            
            // Proceed with actual API call
            finalAuthCode = GenerateRandomAuthorizationCode();
            _logger.LogInformation("Processing salary history request for BorrowerApplicationId {BorrowerApplicationId} with new authorization code", borrowerApplicationId);

            // Build payload
            var payload = new
            {
                authorisationCode = finalAuthCode,
                firstName,
                lastName,
                middleName,
                accountNumber,
                bankCode,
                bvn,
                authorisationChannel = "USSD"
            };

            _logger.LogInformation("Salary History Payload: {Payload}", JsonSerializer.Serialize(payload));

            var endpoint = string.IsNullOrEmpty(_settings.SalaryHistoryEndpoint)
                ? "/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails"
                : _settings.SalaryHistoryEndpoint;

            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending salary history request to: {Url}", requestUrl);
            _logger.LogInformation("Request headers after AddRemitaHeaders: {Headers}",
                string.Join(", ", httpRequest.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaSalaryHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null ||
                    result.Status?.ToLower() == "fail" ||
                    result.ResponseCode != "00" ||
                    !result.HasData ||
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for account {AccountNumber}. Status: {Status}, Code: {Code}, Message: {Message}, HasData: {HasData}",
                        accountNumber, result?.Status, result?.ResponseCode, result?.ResponseMsg, result?.HasData);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved salary history for account: {AccountNumber}, CustomerId: {CustomerId}",
                    accountNumber, result.Data.CustomerId);
                
                // Attach the authorization code to the response for downstream use
                result.AuthorisationCode = finalAuthCode;
                
                // Compute and attach salary statistics if not provided by Remita
                ComputeSalaryStatistics(result.Data);
                
                return result;
            }
            else
            {
                _logger.LogError("Failed to get salary history. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salary history for account: {AccountNumber}", accountNumber);
            return null;
        }
    }

    /// <summary>
    /// Creates a loan mandate in Remita
    /// </summary>
    public async Task<RemitaCreateMandateResponseDto?> CreateMandateAsync(
        string customerId,
        string phoneNumber,
        string accountNumber,
        string loanAmount,
        string collectionAmount,
        string dateOfDisbursement,
        string dateOfCollection,
        string totalCollectionAmount,
        string numberOfRepayments,
        string bankCode,
        string? authorisationCode = null
    )
    {
        try
        {
            // Check if we should use test data for non-live mode
            if (!_settings.UseLiveData)
            {
                _logger.LogInformation("UseLiveData is false - using Remita test data for mandate creation");
                _logger.LogInformation("Original values - Phone: {PhoneNumber}, Account: {AccountNumber}, Bank: {BankCode}",
                    phoneNumber, accountNumber, bankCode);
                
                // Override with Remita test account details
                phoneNumber = "08154567478";
                accountNumber = "5012284010";
                bankCode = "023";
                
                _logger.LogInformation("Test values applied - Phone: {PhoneNumber}, Account: {AccountNumber}, Bank: {BankCode}",
                    phoneNumber, accountNumber, bankCode);
            }
            else
            {
                _logger.LogInformation("UseLiveData is true - using borrower data - Phone: {PhoneNumber}, Account: {AccountNumber}, Bank: {BankCode}",
                    phoneNumber, accountNumber, bankCode);
            }

            // Validate required parameters
            if (string.IsNullOrWhiteSpace(customerId))
            {
                _logger.LogError("CreateMandateAsync: customerId is null or empty");
                throw new ArgumentNullException(nameof(customerId), "Customer ID is required");
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogError("CreateMandateAsync: phoneNumber is null or empty");
                throw new ArgumentNullException(nameof(phoneNumber), "Phone number is required");
            }

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                _logger.LogError("CreateMandateAsync: accountNumber is null or empty");
                throw new ArgumentNullException(nameof(accountNumber), "Account number is required");
            }

            if (string.IsNullOrWhiteSpace(bankCode))
            {
                _logger.LogError("CreateMandateAsync: bankCode is null or empty");
                throw new ArgumentNullException(nameof(bankCode), "Bank code is required");
            }

            if (string.IsNullOrWhiteSpace(authorisationCode))
            {
                _logger.LogError("CreateMandateAsync: authorisationCode is null or empty");
                throw new ArgumentNullException(nameof(authorisationCode), "Authorisation code is required");
            }

            // Normalize phone number (remove +234, spaces, and ensure it starts with 0)
            var normalizedPhoneNumber = NormalizePhoneNumber(phoneNumber);
            _logger.LogInformation("Phone number normalized from {Original} to {Normalized}", phoneNumber, normalizedPhoneNumber);

            // Validate and parse date formats - accepting Remita's expected format (+0000 without colon)
            // Adjust timezone format for parsing: +0000 -> +00:00
            var adjustedDisbursementDate = dateOfDisbursement.Replace("+0000", "+00:00").Replace("-0000", "-00:00");
            if (!DateTime.TryParseExact(adjustedDisbursementDate, "dd-MM-yyyy HH:mm:sszzz", null, System.Globalization.DateTimeStyles.None, out var disbursementDate))
            {
                _logger.LogError("Invalid dateOfDisbursement format: {Date}", dateOfDisbursement);
                throw new ArgumentException($"Invalid date format for disbursement: {dateOfDisbursement}. Expected dd-MM-yyyy HH:mm:ss+0000");
            }

            var adjustedCollectionDate = dateOfCollection.Replace("+0000", "+00:00").Replace("-0000", "-00:00");
            if (!DateTime.TryParseExact(adjustedCollectionDate, "dd-MM-yyyy HH:mm:sszzz", null, System.Globalization.DateTimeStyles.None, out var collectionDate))
            {
                _logger.LogError("Invalid dateOfCollection format: {Date}", dateOfCollection);
                throw new ArgumentException($"Invalid date format for collection: {dateOfCollection}. Expected dd-MM-yyyy HH:mm:ss+0000");
            }

            // Validate numeric values
            if (!decimal.TryParse(loanAmount, out var loanAmountDecimal) || loanAmountDecimal <= 0)
            {
                _logger.LogError("Invalid loanAmount: {Amount}", loanAmount);
                throw new ArgumentException($"Invalid loan amount: {loanAmount}");
            }

            if (!decimal.TryParse(collectionAmount, out var collectionAmountDecimal) || collectionAmountDecimal <= 0)
            {
                _logger.LogError("Invalid collectionAmount: {Amount}", collectionAmount);
                throw new ArgumentException($"Invalid collection amount: {collectionAmount}");
            }

            if (!decimal.TryParse(totalCollectionAmount, out var totalCollectionAmountDecimal) || totalCollectionAmountDecimal <= 0)
            {
                _logger.LogError("Invalid totalCollectionAmount: {Amount}", totalCollectionAmount);
                throw new ArgumentException($"Invalid total collection amount: {totalCollectionAmount}");
            }

            if (!int.TryParse(numberOfRepayments, out var numberOfRepaymentsInt) || numberOfRepaymentsInt <= 0)
            {
                _logger.LogError("Invalid numberOfRepayments: {Count}", numberOfRepayments);
                throw new ArgumentException($"Invalid number of repayments: {numberOfRepayments}");
            }

            // Build payload with proper numeric types
            // Parse the validated numeric strings to actual numbers for JSON serialization
            var payload = new
            {
                customerId,
                authorisationCode,
                authorisationChannel = "USSD",
                phoneNumber = normalizedPhoneNumber,
                accountNumber,
                currency = "NGN",
                loanAmount = loanAmountDecimal,
                collectionAmount = collectionAmountDecimal,
                dateOfDisbursement,
                dateOfCollection,
                totalCollectionAmount = totalCollectionAmountDecimal,
                numberOfRepayments = numberOfRepaymentsInt,
                bankCode
            };

            _logger.LogInformation("Create Mandate Payload: {Payload}", JsonSerializer.Serialize(payload));
            _logger.LogInformation("Create Mandate Details - CustomerId: {CustomerId}, Phone: {Phone}, Account: {Account}, Bank: {BankCode}, Loan: {LoanAmount}, Monthly: {CollectionAmount}, Total: {TotalAmount}, Tenor: {Tenor}",
                customerId, normalizedPhoneNumber, accountNumber, bankCode, loanAmount, collectionAmount, totalCollectionAmount, numberOfRepayments);

            var endpoint = string.IsNullOrEmpty(_settings.CreateMandateEndpoint)
                ? "/loansvc/data/api/v2/payday/post/loan"
                : _settings.CreateMandateEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("=== CREATE MANDATE REQUEST DETAILS ===");
            _logger.LogInformation("Request URL: {Url}", requestUrl);
            _logger.LogInformation("Request Method: POST");
            _logger.LogInformation("Request Headers: {Headers}", 
                string.Join("; ", httpRequest.Headers.Select(h => $"{h.Key}=[{string.Join(",", h.Value)}]")));
            _logger.LogInformation("Content Headers: {ContentHeaders}", 
                string.Join("; ", httpRequest.Content.Headers.Select(h => $"{h.Key}=[{string.Join(",", h.Value)}]")));
            _logger.LogInformation("Request Payload (JSON): {Payload}", JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
            _logger.LogInformation("Request Payload (Raw JSON String): {RawPayload}", JsonSerializer.Serialize(payload));
            _logger.LogInformation("Sending create mandate request for customer: {CustomerId}", customerId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("=== CREATE MANDATE RESPONSE DETAILS ===");
            var statusCodeInt = (int)response.StatusCode;
            var statusCodeText = response.StatusCode.ToString();
            _logger.LogInformation("Response Status Code: {StatusCodeText} ({StatusCodeInt})", statusCodeText, statusCodeInt);
            _logger.LogInformation("Response Headers: {Headers}", 
                string.Join("; ", response.Headers.Select(h => $"{h.Key}=[{string.Join(",", h.Value)}]")));
            _logger.LogInformation("Response Content: {Response}", responseContent);
            _logger.LogInformation("Response Content Length: {Length} bytes", responseContent?.Length ?? 0);

            if (response.IsSuccessStatusCode)
            {
                var result = string.IsNullOrEmpty(responseContent) ? null : JsonSerializer.Deserialize<RemitaCreateMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null ||
                    result.Status?.ToLower() == "fail" ||
                    result.ResponseCode != "00" ||
                    !result.HasData ||
                    result.Data == null)
                {
                    _logger.LogError("=== REMITA RETURNED FAILURE RESPONSE ===");
                    _logger.LogError("Customer ID: {CustomerId}", customerId);
                    _logger.LogError("Status: {Status}", result?.Status);
                    _logger.LogError("Response Code: {Code}", result?.ResponseCode);
                    _logger.LogError("Response Message: {Message}", result?.ResponseMsg);
                    _logger.LogError("Has Data: {HasData}", result?.HasData);
                    _logger.LogError("Full Response Object: {FullResponse}", JsonSerializer.Serialize(result));
                    return null;
                }

                _logger.LogInformation("=== MANDATE CREATED SUCCESSFULLY ===");
                _logger.LogInformation("Customer ID: {CustomerId}", customerId);
                _logger.LogInformation("Mandate Reference: {MandateReference}", result.Data?.MandateReference);
                return result;
            }
            else
            {
                _logger.LogError("=== REMITA API REQUEST FAILED ===");
                var errorStatusCodeInt = (int)response.StatusCode;
                var errorStatusCodeText = response.StatusCode.ToString();
                _logger.LogError("HTTP Status Code: {StatusCodeText} ({StatusCodeInt})", errorStatusCodeText, errorStatusCodeInt);
                _logger.LogError("Reason Phrase: {ReasonPhrase}", response.ReasonPhrase);
                _logger.LogError("Response Body: {Response}", responseContent);
                
                // Try to parse error response as JSON to get more details
                try
                {
                    var errorResponse = string.IsNullOrEmpty(responseContent) ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (errorResponse != null)
                    {
                        _logger.LogError("Parsed Error Response: {ErrorDetails}", 
                            string.Join(", ", errorResponse.Select(kvp => $"{kvp.Key}={kvp.Value}")));
                    }
                }
                catch
                {
                    _logger.LogError("Could not parse error response as JSON. Raw response logged above.");
                }
                
                return null;
            }
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError("=== VALIDATION ERROR ===");
            _logger.LogError(argEx, "Parameter validation failed for customer: {CustomerId}. Error: {ErrorMessage}", 
                customerId, argEx.Message);
            throw; // Re-throw validation errors so they bubble up
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError("=== HTTP REQUEST ERROR ===");
            _logger.LogError(httpEx, "HTTP request failed for customer: {CustomerId}. Message: {Message}", 
                customerId, httpEx.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("=== UNEXPECTED ERROR ===");
            _logger.LogError(ex, "Unexpected error creating mandate for customer: {CustomerId}. Type: {ExceptionType}, Message: {Message}", 
                customerId, ex.GetType().Name, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Stops an existing loan mandate in Remita
    /// </summary>
    public async Task<RemitaStopMandateResponseDto?> StopMandateAsync(
        string customerId,
        string mandateReference,
        string? authorisationCode)
    {
        try
        {
            // Build payload
            var payload = new
            {
                authorisationCode,
                customerId,
                mandateReference
            };

            var endpoint = string.IsNullOrEmpty(_settings.StopMandateEndpoint)
                ? "/loansvc/data/api/v2/payday/stop/loan"
                : _settings.StopMandateEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending stop mandate request for customer: {CustomerId}, Mandate: {MandateReference}",
                customerId, mandateReference);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaStopMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null ||
                    result.Status?.ToLower() == "fail" ||
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for stopping mandate {MandateReference}. Status: {Status}, Message: {Message}",
                        mandateReference, result?.Status, result?.Data?.Status);
                    return null;
                }

                _logger.LogInformation("Successfully stopped mandate for customer: {CustomerId}", customerId);
                return result;
            }
            else
            {
                _logger.LogError("Failed to stop mandate. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping mandate for customer: {CustomerId}", customerId);
            return null;
        }
    }

    /// <summary>
    /// Gets mandate payment history from Remita
    /// </summary>
    public async Task<RemitaMandateHistoryResponseDto?> GetMandateHistoryAsync(
        string customerId,
        string mandateReference,
        string? authorisationCode)
    {
        try
        {
            // Build payload
            var payload = new
            {
                authorisationCode,
                customerId,
                mandateRef = mandateReference
            };

            var endpoint = string.IsNullOrEmpty(_settings.MandateHistoryEndpoint)
                ? "/loansvc/data/api/v2/payday/loan/payment/history"
                : _settings.MandateHistoryEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending mandate history request for customer: {CustomerId}, Mandate: {MandateReference}",
                customerId, mandateReference);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaMandateHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null ||
                    result.Status?.ToLower() == "fail" ||
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for mandate history {MandateReference}. Status: {Status}",
                        mandateReference, result?.Status);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved mandate history for customer: {CustomerId}", customerId);
                return result;
            }
            else
            {
                _logger.LogError("Failed to get mandate history. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mandate history for customer: {CustomerId}", customerId);
            return null;
        }
    }

    public Task<BanksResponseDto?> GetBanksAsync()
    {
        throw new NotImplementedException();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Direct Debit Mandate API  (echannelsvc/echannel/mandate/)
    // ══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Step 1 – Creates a new Direct Debit mandate in Remita.
    /// Endpoint: POST /echannelsvc/echannel/mandate/setup
    /// Hash = SHA-512(merchantId + serviceTypeId + requestId + amount + apiKey)
    /// </summary>
    public async Task<DirectDebitGenerateMandateResponseDto?> GenerateDirectDebitMandateAsync(
        DirectDebitGenerateMandateRequestDto request)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(request.PayerPhone);

            // requestId is a unique timestamp-based value, same pattern used across the service
            var requestId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

            // Amount must be sent as a plain string (no decimals) per Remita's spec
            var amountStr = request.Amount.ToString("F2");

            // hash = SHA-512(merchantId + serviceTypeId + requestId + amount + apiKey)
            var hashInput  = $"{_settings.MerchantId}{_settings.ServiceTypeId}{requestId}{amountStr}{_settings.ApiKey}";
            var hash       = ComputeSha512Hash(hashInput);

            var payload = new
            {
                merchantId    = _settings.MerchantId,
                serviceTypeId = _settings.ServiceTypeId,
                requestId,
                hash,
                payerName     = request.PayerName,
                payerEmail    = request.PayerEmail,
                payerPhone    = normalizedPhone,
                payerBankCode = request.PayerBankCode,
                payerAccount  = request.PayerAccountNumber,
                amount        = "10000",
                startDate     = request.StartDate,
                endDate       = request.EndDate,
                mandateType   = request.MandateType,
                frequency     = request.Frequency
            };

            var endpoint = string.IsNullOrEmpty(_settings.GenerateMandateEndpoint)
                ? "/echannelsvc/echannel/mandate/setup"
                : _settings.GenerateMandateEndpoint;

            var requestUrl = BuildUrl(endpoint);
            _logger.LogInformation("[DirectDebit] GenerateMandate → {Url} | MerchantId={MerchantId}, ServiceTypeId={ServiceTypeId}, RequestId={RequestId}, Amount={Amount}, PayerAccount={Account}",
                requestUrl, _settings.MerchantId, _settings.ServiceTypeId, requestId, amountStr, request.PayerAccountNumber);

            _logger.LogInformation("[DirectDebit] GenerateMandate Payload: {Payload}", JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            AddRemitaHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[DirectDebit] GenerateMandate ← HTTP {StatusCode} | Body: {Body}",
                (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[DirectDebit] GenerateMandate failed. HTTP {Status} | Body: {Body}",
                    (int)response.StatusCode, responseContent);
                return new DirectDebitGenerateMandateResponseDto
                {
                    StatusCode = ((int)response.StatusCode).ToString(),
                    Message    = $"Remita returned HTTP {(int)response.StatusCode}. Please verify your bank details and try again."
                };
            }

            // Guard against non-JSON responses (e.g. HTML error pages from Remita's server)
            var trimmed = responseContent.TrimStart();
            if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
            {
                _logger.LogError("[DirectDebit] GenerateMandate received a non-JSON response. Body: {Body}", responseContent);
                return new DirectDebitGenerateMandateResponseDto
                {
                    StatusCode = "ERR",
                    Message    = "Remita returned an unexpected response. Please try again later."
                };
            }

            var result = JsonSerializer.Deserialize<DirectDebitGenerateMandateResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.StatusCode != "00")
            {
                _logger.LogWarning("[DirectDebit] GenerateMandate: Remita returned non-success. StatusCode={Code}, Message={Msg}",
                    result?.StatusCode, result?.Message);
                return result; // Return it so the caller can surface Remita's message
            }

            _logger.LogInformation("[DirectDebit] GenerateMandate succeeded. MandateId={MandateId}",
                result.Data?.MandateId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DirectDebit] GenerateMandate threw an unexpected error");
            return null;
        }
    }

    /// <summary>
    /// Step 2a – Sends an OTP to the mandate holder's phone to initiate activation.
    /// Endpoint: POST /echannelsvc/echannel/mandate/requestAuthorization
    /// </summary>
    public async Task<DirectDebitRequestAuthorizationResponseDto?> RequestMandateAuthorizationAsync(
        DirectDebitRequestAuthorizationDto request)
    {
        try
        {
            var payload = new
            {
                mandateId   = request.MandateId,
                phoneNumber = NormalizePhoneNumber(request.PhoneNumber),
                requestId   = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
            };

            var endpoint = string.IsNullOrEmpty(_settings.ActivateMandateOtpEndpoint)
                ? "/echannelsvc/echannel/mandate/requestAuthorization"
                : _settings.ActivateMandateOtpEndpoint;

            var requestUrl = BuildUrl(endpoint);
            _logger.LogInformation("[DirectDebit] RequestAuthorization → {Url} | MandateId={MandateId}",
                requestUrl, request.MandateId);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            AddRemitaHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[DirectDebit] RequestAuthorization ← HTTP {StatusCode} | Body: {Body}",
                (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[DirectDebit] RequestAuthorization failed. Status: {Status}", response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<DirectDebitRequestAuthorizationResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.StatusCode != "00")
            {
                _logger.LogWarning("[DirectDebit] RequestAuthorization: non-success from Remita. StatusCode={Code}, Message={Msg}",
                    result?.StatusCode, result?.Message);
            }
            else
            {
                _logger.LogInformation("[DirectDebit] OTP dispatched successfully for MandateId={MandateId}", request.MandateId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DirectDebit] RequestAuthorization threw an unexpected error. MandateId={MandateId}",
                request.MandateId);
            return null;
        }
    }

    /// <summary>
    /// Step 2b – Validates the OTP, completing mandate activation.
    /// Endpoint: POST /echannelsvc/echannel/mandate/validateAuthorization
    /// </summary>
    public async Task<DirectDebitValidateAuthorizationResponseDto?> ValidateMandateAuthorizationAsync(
        DirectDebitValidateAuthorizationDto request)
    {
        try
        {
            // Build the authParams array in the format Remita expects
            var authParams = request.AuthParams.Select(p => new
            {
                param1 = p.Param1,
                param2 = p.Param2,
                value  = p.Value
            }).ToList();

            var payload = new
            {
                remitaTransRef = request.RemitaTransRef,
                authParams
            };

            var endpoint = string.IsNullOrEmpty(_settings.ValidateMandateOtpEndpoint)
                ? "/echannelsvc/echannel/mandate/validateAuthorization"
                : _settings.ValidateMandateOtpEndpoint;

            var requestUrl = BuildUrl(endpoint);
            _logger.LogInformation("[DirectDebit] ValidateAuthorization → {Url} | RemitaTransRef={TransRef}",
                requestUrl, request.RemitaTransRef);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            AddRemitaHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[DirectDebit] ValidateAuthorization ← HTTP {StatusCode} | Body: {Body}",
                (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[DirectDebit] ValidateAuthorization failed. Status: {Status}", response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<DirectDebitValidateAuthorizationResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.StatusCode != "00")
            {
                _logger.LogWarning("[DirectDebit] ValidateAuthorization: non-success from Remita. StatusCode={Code}, Message={Msg}",
                    result?.StatusCode, result?.Message);
            }
            else
            {
                _logger.LogInformation("[DirectDebit] Mandate activated successfully. MandateRef={Ref}",
                    result.MandateRef);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DirectDebit] ValidateAuthorization threw an unexpected error. RemitaTransRef={TransRef}",
                request.RemitaTransRef);
            return null;
        }
    }

    /// <summary>
    /// Cancels an active Direct Debit mandate.
    /// Endpoint: POST /echannelsvc/echannel/mandate/stop
    /// </summary>
    public async Task<DirectDebitStopMandateResponseDto?> StopDirectDebitMandateAsync(
        DirectDebitStopMandateRequestDto request)
    {
        try
        {
            var payload = new
            {
                mandateId = request.MandateId,
                requestId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()
            };

            var endpoint = string.IsNullOrEmpty(_settings.StopDirectDebitMandateEndpoint)
                ? "/echannelsvc/echannel/mandate/stop"
                : _settings.StopDirectDebitMandateEndpoint;

            var requestUrl = BuildUrl(endpoint);
            _logger.LogInformation("[DirectDebit] StopMandate → {Url} | MandateId={MandateId}",
                requestUrl, request.MandateId);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            AddRemitaHeaders(httpRequest);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[DirectDebit] StopMandate ← HTTP {StatusCode} | Body: {Body}",
                (int)response.StatusCode, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[DirectDebit] StopMandate failed. Status: {Status}", response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<DirectDebitStopMandateResponseDto>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || result.StatusCode != "00")
            {
                _logger.LogWarning("[DirectDebit] StopMandate: non-success from Remita. StatusCode={Code}, Message={Msg}",
                    result?.StatusCode, result?.Message);
            }
            else
            {
                _logger.LogInformation("[DirectDebit] Mandate stopped successfully. MandateId={MandateId}",
                    request.MandateId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DirectDebit] StopMandate threw an unexpected error. MandateId={MandateId}",
                request.MandateId);
            return null;
        }
    }

    /// <summary>
    /// Processes and saves loan collection notification from Remita webhook
    /// </summary>
    // Private Helper Methods
    private static string GenerateRandomAuthorizationCode()
    {
        // Generate a random 5-digit authorization code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    private string GenerateCustomerId()
    {
        // Generate customer ID using timestamp (like Date.now() in JavaScript)
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogError("BaseUrl is not configured in RemitaSettings");
            throw new InvalidOperationException("Remita BaseUrl is not configured");
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        _logger.LogDebug("Built URL: {Url}", url);
        return url;
    }

    private string BuildAuthorizationToken(string requestId)
    {
        var hashInput = $"{_settings.ApiKey}{requestId}{_settings.ApiToken}";
        _logger.LogInformation("Hash Input: ApiKey={ApiKey}, RequestId={RequestId}, ApiToken={ApiToken}",
            _settings.ApiKey, requestId, _settings.ApiToken);
        _logger.LogInformation("Concatenated Hash Input: {HashInput}", hashInput);
        var hash = ComputeSha512Hash(hashInput);
        _logger.LogInformation("Computed Hash: {Hash}", hash);
        return $"remitaConsumerKey={_settings.ApiKey}, remitaConsumerToken={hash}";
    }

    private void AddRemitaHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Generate dynamic REQUEST_ID as timestamp (like Postman does) - use same value for header and hash
        var requestId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        _logger.LogInformation("Adding Remita headers - API_KEY: {ApiKey}, MERCHANT_ID: {MerchantId}, REQUEST_ID: {RequestId}",
            _settings.ApiKey, _settings.MerchantId, requestId);

        request.Headers.TryAddWithoutValidation("API_KEY", _settings.ApiKey);
        request.Headers.TryAddWithoutValidation("MERCHANT_ID", _settings.MerchantId);
        request.Headers.TryAddWithoutValidation("REQUEST_ID", requestId);

        // Pass the same requestId to BuildAuthorizationToken
        var authToken = BuildAuthorizationToken(requestId);
        _logger.LogInformation("Generated Authorization Token: {AuthToken}", authToken);

        // Use TryAddWithoutValidation for AUTHORIZATION header because it contains a comma
        var added = request.Headers.TryAddWithoutValidation("AUTHORIZATION", authToken);
        _logger.LogInformation("AUTHORIZATION header added: {Added}", added);

        // Log all headers for debugging
        _logger.LogInformation("All request headers: {Headers}", string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
    }

    private static string ComputeSha512Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA512.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }

    /// <summary>
    /// Normalizes phone number to Nigerian format expected by Remita
    /// Removes +234, spaces, dashes and ensures it starts with 0
    /// </summary>
    private static string NormalizePhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        // Remove all non-digit characters
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Handle international format (+234 or 234)
        if (digitsOnly.StartsWith("234"))
        {
            digitsOnly = "0" + digitsOnly.Substring(3);
        }

        // Ensure it starts with 0
        if (!digitsOnly.StartsWith("0") && digitsOnly.Length == 10)
        {
            digitsOnly = "0" + digitsOnly;
        }

        return digitsOnly;
    }

    /// <summary>
    /// Computes salary statistics and loan information from the raw Remita response
    /// and populates the pre-computed fields on the DTO
    /// </summary>
    private void ComputeSalaryStatistics(RemitaSalaryDataDto data)
    {
        if (data == null) return;

        var payments = data.SalaryPaymentDetails ?? new List<RemitaSalaryPaymentDto>();
        var amounts = new List<decimal>();

        foreach (var payment in payments)
        {
            if (decimal.TryParse(payment.Amount, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out var amount))
            {
                amounts.Add(amount);
            }
        }

        // Compute salary statistics if not already set
        if (string.IsNullOrEmpty(data.MaxSalaryAmount) && amounts.Any())
        {
            data.MaxSalaryAmount = amounts.Max().ToString("F2");
        }
        if (string.IsNullOrEmpty(data.MinSalaryAmount) && amounts.Any())
        {
            data.MinSalaryAmount = amounts.Min().ToString("F2");
        }
        if (string.IsNullOrEmpty(data.AverageMonthlySalary) && amounts.Any())
        {
            data.AverageMonthlySalary = amounts.Average().ToString("F2");
        }
        if (string.IsNullOrEmpty(data.LatestSalaryAmount) && amounts.Any())
        {
            data.LatestSalaryAmount = amounts.First().ToString("F2");
        }
        if (string.IsNullOrEmpty(data.ConsistentMonths))
        {
            data.ConsistentMonths = data.SalaryCount;
        }

        // Compute loan information
        var loanHistory = data.LoanHistoryDetails ?? new List<RemitaLoanHistoryDto>();
        var totalOutstanding = loanHistory
            .Where(l => l.OutstandingAmount > 0)
            .Sum(l => l.OutstandingAmount);

        if (string.IsNullOrEmpty(data.TotalOutstandingAmount))
        {
            data.TotalOutstandingAmount = totalOutstanding.ToString("F2");
        }
        if (string.IsNullOrEmpty(data.HasOutstandingLoans))
        {
            data.HasOutstandingLoans = (totalOutstanding > 0).ToString();
        }

        _logger.LogInformation("Computed salary stats - MaxSalary: {Max}, TotalOutstanding: {Outstanding}, HasLoans: {HasLoans}",
            data.MaxSalaryAmount, data.TotalOutstandingAmount, data.HasOutstandingLoans);
    }
}
