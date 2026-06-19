using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

public class MonoService : IMonoService
{
    private readonly MonoSettings _settings = null!;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MonoService> _logger;
    private readonly ICombinedRepository _cRepo;
    private readonly ApplicationDbContext _db;

    public MonoService(
        IOptions<MonoSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<MonoService> logger,
        ICombinedRepository cRepo,
        ApplicationDbContext db)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _cRepo = cRepo;
        _db = db;
        
        // DIAGNOSTIC LOGGING - Constructor
        _logger.LogInformation("MonoService initialized");
        _logger.LogInformation("  BaseUrl: {BaseUrl}", _settings?.BaseUrl ?? "NULL");
        _logger.LogInformation("  SecretKey loaded: {HasKey}", !string.IsNullOrEmpty(_settings?.SecretKey));
        _logger.LogInformation("  SecretKey length: {Length}", _settings?.SecretKey?.Length ?? 0);
        _logger.LogInformation("  PublicKey loaded: {HasKey}", !string.IsNullOrEmpty(_settings?.PublicKey));
    }

    public async Task<MonoCreateCustomerResponseDto?> CreateCustomerAsync(MonoCreateCustomerRequestDto request, string? userId = null)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/v2/customers"))
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Creating Mono customer for email: {Email}", request.Email);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono create customer response: {StatusCode} - {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<MonoCreateCustomerResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Mono customer created successfully. Id: {CustomerId}", result?.Data?.Id);
                return result;
            }

            // Handle "customer already exists" conflict — extract and reuse the existing customer ID
            var conflictResponse = JsonSerializer.Deserialize<MonoCustomerConflictResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var existingId = conflictResponse?.Data?.ExistingCustomer?.Id;
            if (!string.IsNullOrEmpty(existingId))
            {
                _logger.LogInformation("Mono customer already exists. Reusing existing customer ID: {CustomerId}", existingId);
                return new MonoCreateCustomerResponseDto
                {
                    Status = "successful",
                    Message = "Customer already exists — existing customer reused",
                    Data = new MonoCustomerDataDto { Id = existingId }
                };
            }

            _logger.LogError("Mono customer creation failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Mono customer for email: {Email}", request.Email);
            return null;
        }
    }

    public async Task<MonoGetCustomerResponseDto?> GetCustomerByIdAsync(string customerId, string? userId = null)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/v2/customers/{customerId}"));
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Fetching Mono customer: {CustomerId}", customerId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono get customer response: {StatusCode} - {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<MonoGetCustomerResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode)
                _logger.LogError("Mono get customer failed: {StatusCode} - {Content}", response.StatusCode, responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Mono customer: {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<MonoGetAllCustomersResponseDto?> GetAllCustomersAsync(int page = 1, int limit = 20, string? userId = null)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/v2/customers?page={page}&limit={limit}"));
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Fetching all Mono customers. Page: {Page}, Limit: {Limit}", page, limit);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono get all customers response: {StatusCode} - {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<MonoGetAllCustomersResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode)
                _logger.LogError("Mono get all customers failed: {StatusCode} - {Content}", response.StatusCode, responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Mono customers");
            return null;
        }
    }

    public async Task<MonoGetLinkedAccountsResponseDto?> GetCustomerLinkedAccountsAsync(string customerId, string? userId = null)
    {
        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"/v2/customers/{customerId}/accounts"));
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Fetching linked accounts for Mono customer: {CustomerId}", customerId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono linked accounts response: {StatusCode} - {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<MonoGetLinkedAccountsResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode)
                _logger.LogError("Mono get linked accounts failed: {StatusCode} - {Content}", response.StatusCode, responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching linked accounts for Mono customer: {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<MonoGenerateMandateResponseDto?> GenerateMandateAsync(Guid loanId, MonoGenerateMandateRequestDto request, string? userId = null)
    {
        try
        {
            // Get loan information
            var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);
            if (loan == null)
            {
                _logger.LogError("Loan not found: {LoanId}", loanId);
                return null;
            }

            // Get borrower application for account information
            var borrowerApplication = await _db.BorrowerApplications
                .FirstOrDefaultAsync(ba => ba.LoanId == loanId);

            if (borrowerApplication == null)
            {
                _logger.LogError("Borrower application not found for loan: {LoanId}", loanId);
                return null;
            }

            // reference: ≤24 alphanumeric chars (no hyphens)
            var rawRef = !string.IsNullOrEmpty(request.Reference) ? request.Reference : Guid.NewGuid().ToString("N");
            var safeReference = new string(rawRef.Where(char.IsLetterOrDigit).ToArray())[..Math.Min(24, rawRef.Length)];

            // Build the request payload
            var payload = new MonoGenerateMandateRequestDto
            {
                Type = "recurring-debit",
                Method = "mandate",
                MandateType = "emandate",
                DebitType = request.DebitType ?? "variable",
                Customer = new MonoMandateCustomerDto
                {
                    Id = !string.IsNullOrEmpty(request.Customer?.Id) ? request.Customer.Id : _settings.TestCustomerId
                },
                Amount = request.Amount,
                Reference = safeReference,
                AccountNumber = !string.IsNullOrEmpty(request.AccountNumber) ? request.AccountNumber : borrowerApplication.AccountNo ?? string.Empty,
                BankCode = !string.IsNullOrEmpty(request.BankCode) ? request.BankCode : borrowerApplication.BankCode ?? string.Empty,
                Description = !string.IsNullOrEmpty(request.Description) ? request.Description : $"Loan mandate for {loan.User?.Email ?? "borrower"}",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RedirectUrl = request.RedirectUrl,
                Meta = new { source = "devpay", loanId = loanId.ToString() }
            };

            // Create HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/v2/payments/initiate"))
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            // Add headers
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Sending Mono mandate request for loan: {LoanId}", loanId);

            // Send request
            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Mono mandate request failed: {Status} - {Reason}. Response: {Response}",
                    response.StatusCode, response.ReasonPhrase, responseContent);

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                    _logger.LogError("Mono API Error: {Message}", errorResponse?.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse Mono error response");
                }

                return null;
            }

            // Parse successful response
            var result = JsonSerializer.Deserialize<MonoGenerateMandateResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Data != null)
            {
                // Update loan with Mono mandate information
                loan.MandateRef = result.Data.Id;
                loan.IsMandateCreated = true;
                loan.MandateCreatedAt = DateTime.UtcNow;
                await _cRepo.UpdateLoanAsync(loan);

                // Save mandate reference to database
                await SaveMandateReferenceAsync(loan.CompanyId, loanId, result.Data);

                _logger.LogInformation("Successfully created Mono mandate for loan {LoanId}: {MandateRef}",
                    loanId, result.Data.Id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating mandate for loan {LoanId}: {Message}", loanId, ex.Message);
            return null;
        }
    }

    public async Task<MonoCancelMandateResponseDto?> CancelMandateAsync(string mandateId, string? userId = null)
    {
        try
        {
            var requestUri = $"/v3/payments/mandates/{mandateId}/cancel";

            var request = new HttpRequestMessage(HttpMethod.Patch, BuildUrl(requestUri));

            // Add headers using the existing method
            AddMonoHeaders(request);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono cancel mandate response for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var cancelResponse = JsonSerializer.Deserialize<MonoCancelMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return cancelResponse;
            }

            _logger.LogError("Mono cancel mandate failed for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling mandate {MandateId}: {Message}", mandateId, ex.Message);
            return null;
        }
    }

    public async Task<MonoPauseMandateResponseDto?> PauseMandateAsync(string mandateId, string? userId = null)
    {
        try
        {
            var requestUri = $"/v3/payments/mandates/{mandateId}/pause";

            var request = new HttpRequestMessage(HttpMethod.Patch, BuildUrl(requestUri));

            // Add headers using the existing method
            AddMonoHeaders(request);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono pause mandate response for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var pauseResponse = JsonSerializer.Deserialize<MonoPauseMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return pauseResponse;
            }

            _logger.LogError("Mono pause mandate failed for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing mandate {MandateId}: {Message}", mandateId, ex.Message);
            return null;
        }
    }

    public async Task<MonoReinstateMandateResponseDto?> ReinstateMandateAsync(string mandateId, string? userId = null)
    {
        try
        {
            var requestUri = $"/v3/payments/mandates/{mandateId}/reinstate";

            var request = new HttpRequestMessage(HttpMethod.Patch, BuildUrl(requestUri));

            // Add headers using the existing method
            AddMonoHeaders(request);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono reinstate mandate response for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var reinstateResponse = JsonSerializer.Deserialize<MonoReinstateMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return reinstateResponse;
            }

            _logger.LogError("Mono reinstate mandate failed for mandate {MandateId}: {StatusCode} - {Content}",
                mandateId, response.StatusCode, responseContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reinstating mandate {MandateId}: {Message}", mandateId, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Manually triggers a collection debit against an active mandate.
    /// Endpoint: POST /v3/payments/mandates/{mandateId}/debit
    /// </summary>
    public async Task<MonoInitiateDebitResponseDto?> InitiateDebitAsync(
        string mandateId,
        MonoInitiateDebitRequestDto request,
        string? userId = null)
    {
        try
        {
            var requestUri = $"/v3/payments/mandates/{mandateId}/debit";

            // Only include fields that are set — amount is optional for fixed mandates
            object payload = request.Amount.HasValue
                ? new { amount = request.Amount.Value, description = request.Description ?? string.Empty }
                : new { description = request.Description ?? string.Empty };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(requestUri))
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);

            _logger.LogInformation("[Mono] InitiateDebit → {Uri} | MandateId={MandateId}, Amount={Amount}",
                requestUri, mandateId, request.Amount?.ToString() ?? "default");

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[Mono] InitiateDebit ← HTTP {StatusCode} | MandateId={MandateId} | Body: {Body}",
                (int)response.StatusCode, mandateId, responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("[Mono] InitiateDebit failed for mandate {MandateId}: {StatusCode} - {Content}",
                    mandateId, response.StatusCode, responseContent);

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                    _logger.LogError("[Mono] InitiateDebit API error: {Message}", errorResponse?.Message);
                }
                catch { /* ignore parse failure */ }

                return null;
            }

            var result = JsonSerializer.Deserialize<MonoInitiateDebitResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("[Mono] InitiateDebit succeeded for MandateId={MandateId}, DebitId={DebitId}, Status={Status}",
                mandateId, result?.Data?.Id, result?.Data?.Status);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Mono] InitiateDebit threw an unexpected error for mandate {MandateId}", mandateId);
            return null;
        }
    }

    public async Task<MonoBanksResponseDto?> GetBanksAsync()
    {
        try
        {
            // DIAGNOSTIC LOGGING - Start
            _logger.LogInformation("=== MONO API DIAGNOSTIC START ===");
            _logger.LogInformation("MonoSettings.BaseUrl: {BaseUrl}", _settings.BaseUrl ?? "NULL");
            _logger.LogInformation("MonoSettings.SecretKey exists: {HasKey}", !string.IsNullOrEmpty(_settings.SecretKey));
            _logger.LogInformation("MonoSettings.SecretKey length: {Length}", _settings.SecretKey?.Length ?? 0);
            _logger.LogInformation("MonoSettings.SecretKey first 15 chars: {Prefix}", 
                !string.IsNullOrEmpty(_settings.SecretKey) && _settings.SecretKey.Length >= 15 
                    ? _settings.SecretKey.Substring(0, 15) + "..." 
                    : _settings.SecretKey ?? "NULL");
            
            var requestUri = "/v3/banks/list";
            var fullUrl = BuildUrl(requestUri);
            
            _logger.LogInformation("Full URL: {Url}", fullUrl);

            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);

            // Add headers using the existing method
            AddMonoHeaders(request);
            
            // LOG ALL HEADERS
            _logger.LogInformation("Request Headers:");
            foreach (var header in request.Headers)
            {
                _logger.LogInformation("  {Key}: {Value}", header.Key, string.Join(", ", header.Value));
            }
            _logger.LogInformation("=== MONO API DIAGNOSTIC END ===");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono get banks response: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var banksResponse = JsonSerializer.Deserialize<MonoBanksResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return banksResponse;
            }

            _logger.LogError("Mono get banks failed: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching banks from Mono: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<MonoNinLookupResponseDto?> NinLookupAsync(string nin, string? userId = null)
    {
        try
        {
            var payload = new MonoNinLookupRequestDto { Nin = nin };
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/v3/lookup/nin"))
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);

            _logger.LogInformation("[Mono NIN Lookup] Initiating lookup for NIN: {Nin}", nin);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[Mono NIN Lookup] Response: {StatusCode} - {Content}", response.StatusCode, responseContent);

            var result = JsonSerializer.Deserialize<MonoNinLookupResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (!response.IsSuccessStatusCode)
                _logger.LogError("[Mono NIN Lookup] Failed: {StatusCode} - {Content}", response.StatusCode, responseContent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Mono NIN Lookup] Error performing NIN lookup for: {Nin}", nin);
            return null;
        }
    }

    public async Task<MonoBvnLookupResponseDto?> BvnLookupAsync(MonoBvnLookupRequestDto request, string? userId = null)
    {
        try
        {
            var requestUri = "/v2/lookup/bvn/initiate";
            var requestBody = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(requestUri))
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            // Add headers using the existing method
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("[Mono BVN Lookup] REQUEST - URI: {URI}, Body: {Body}", 
                requestUri, requestBody);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[Mono BVN Lookup] RESPONSE - Status: {StatusCode}, Body: {Content}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var bvnResponse = JsonSerializer.Deserialize<MonoBvnLookupResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return bvnResponse;
            }

            _logger.LogError("Mono BVN lookup failed: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                _logger.LogError("Mono BVN Lookup API Error: {Message}", errorResponse?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Mono BVN lookup error response");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing BVN lookup with Mono: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<MonoBvnVerifyResponseDto?> BvnVerifyAsync(MonoBvnVerifyRequestDto request, string sessionId, string? userId = null)
    {
        try
        {
            var requestUri = "/v2/lookup/bvn/verify";
            var requestBody = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(requestUri))
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);
            httpRequest.Headers.Add("x-session-id", sessionId);

            _logger.LogInformation("[Mono BVN Verify] REQUEST - URI: {URI}, SessionId: {SessionId}, Body: {Body}", 
                requestUri, sessionId, requestBody);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[Mono BVN Verify] RESPONSE - Status: {StatusCode}, Body: {Content}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var verifyResponse = JsonSerializer.Deserialize<MonoBvnVerifyResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return verifyResponse;
            }

            _logger.LogError("Mono BVN verify failed: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                _logger.LogError("Mono BVN Verify API Error: {Message}", errorResponse?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Mono BVN verify error response");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying BVN with Mono: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<MonoBvnDetailsResponseDto?> BvnGetDetailsAsync(MonoBvnDetailsRequestDto request, string sessionId, string? userId = null)
    {
        try
        {
            var requestUri = "/v2/lookup/bvn/details";
            // Mask OTP in logs for security
            var maskedRequest = new { otp = "****" };
            var requestBody = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(requestUri))
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);
            httpRequest.Headers.Add("x-session-id", sessionId);

            _logger.LogInformation("[Mono BVN Details] REQUEST - URI: {URI}, SessionId: {SessionId}, Body: {Body}", 
                requestUri, sessionId, JsonSerializer.Serialize(maskedRequest));

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("[Mono BVN Details] RESPONSE - Status: {StatusCode}, Body: {Content}", 
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var detailsResponse = JsonSerializer.Deserialize<MonoBvnDetailsResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return detailsResponse;
            }

            _logger.LogError("Mono BVN details failed: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                _logger.LogError("Mono BVN Details API Error: {Message}", errorResponse?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Mono BVN details error response");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BVN details from Mono: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<MonoBvnValidationResultDto?> ValidateBvnCompleteAsync(string bvn, string method, string phoneNumber, string otp, string scope = "identity", string? userId = null)
    {
        try
        {
            _logger.LogInformation("Starting complete BVN validation workflow for BVN: {BVN}", bvn);

            var result = new MonoBvnValidationResultDto
            {
                Bvn = bvn,
                IsValidated = false
            };

            // Step 1: Initiate BVN lookup
            var initiateResponse = await BvnLookupAsync(new MonoBvnLookupRequestDto
            {
                Bvn = bvn,
                Scope = scope
            }, userId);

            if (initiateResponse?.Data == null)
            {
                result.ValidationMessage = "Failed to initiate BVN lookup";
                return result;
            }

            result.SessionId = initiateResponse.Data.SessionId;
            result.AvailableMethods = initiateResponse.Data.Methods;

            // Step 2: Verify with selected method
            var verifyResponse = await BvnVerifyAsync(new MonoBvnVerifyRequestDto
            {
                Method = method,
                PhoneNumber = phoneNumber
            }, result.SessionId, userId);

            if (verifyResponse == null || verifyResponse.Status != "successful")
            {
                result.ValidationMessage = verifyResponse?.Message ?? "BVN verification failed";
                return result;
            }

            // Step 3: Get BVN details with OTP
            var detailsResponse = await BvnGetDetailsAsync(new MonoBvnDetailsRequestDto
            {
                Otp = otp
            }, result.SessionId, userId);

            if (detailsResponse?.Data != null && detailsResponse.Status == "successful")
            {
                result.BvnDetails = detailsResponse.Data;
                result.IsValidated = true;
                result.ValidationMessage = "BVN validation completed successfully";

                _logger.LogInformation("BVN validation completed successfully for BVN: {BVN}", bvn);
            }
            else
            {
                result.ValidationMessage = detailsResponse?.Message ?? "Failed to retrieve BVN details";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in complete BVN validation workflow: {Message}", ex.Message);
            return new MonoBvnValidationResultDto
            {
                Bvn = bvn,
                IsValidated = false,
                ValidationMessage = $"Validation error: {ex.Message}"
            };
        }
    }

    // Session-based BVN Validation Methods
    public async Task<MonoBvnVerifyResponseDto?> BvnVerifyWithSessionAsync(MonoBvnSessionRequestDto request, string? userId = null)
    {
        return await BvnVerifyAsync(new MonoBvnVerifyRequestDto
        {
            Method = request.Method,
            PhoneNumber = request.PhoneNumber
        }, request.SessionId, userId);
    }

    public async Task<MonoBvnDetailsResponseDto?> BvnGetDetailsWithSessionAsync(MonoBvnSessionDetailsRequestDto request, string? userId = null)
    {
        return await BvnGetDetailsAsync(new MonoBvnDetailsRequestDto
        {
            Otp = request.Otp
        }, request.SessionId, userId);
    }

    // BVN Verification Tracking Methods
    public async Task<bool> IsBvnAlreadyVerifiedAsync(string bvn)
    {
        try
        {
            var bvnHash = GenerateBvnHash(bvn);
            var existingRecord = await _db.MonoBvnVerificationRecords
                .Where(r => r.BvnHash == bvnHash && r.IsVerified && r.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            return existingRecord != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking BVN verification status");
            return false;
        }
    }

    public async Task<MonoBvnVerificationRecord?> GetBvnVerificationRecordAsync(string bvn)
    {
        try
        {
            var bvnHash = GenerateBvnHash(bvn);
            return await _db.MonoBvnVerificationRecords
                .Where(r => r.BvnHash == bvnHash && r.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving BVN verification record");
            return null;
        }
    }

    // Credit History Methods
    public async Task<MonoCreditHistoryResponseDto?> GetCreditHistoryAsync(string bvn, string provider = "cdc", string? userId = null)
    {
        _logger.LogInformation("Getting credit history for BVN: {BvnMasked} with provider: {Provider}", bvn, provider);
        try
        {
            // Validate provider
            if (provider != "xds" && provider != "cdc" && provider != "all")
            {
                _logger.LogError("Invalid credit history provider: {Provider}. Must be 'xds' or 'cdc'", provider);
                return null;
            }

            var requestUri = $"/v3/lookup/credit-history/{provider}";
            var requestPayload = new MonoCreditHistoryRequestDto { Bvn = bvn };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl(requestUri))
            {
                Content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json")
            };

            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Sending Mono credit history request for BVN: {BvnMasked} with provider: {Provider}",
                MaskBvn(bvn), provider);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Mono credit history response: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var creditHistoryResponse = JsonSerializer.Deserialize<MonoCreditHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return creditHistoryResponse;
            }

            _logger.LogError("Mono credit history failed: {StatusCode} - {Content}",
                response.StatusCode, responseContent);

            // Try to parse error response
            try
            {
                var errorResponse = JsonSerializer.Deserialize<MonoErrorResponseDto>(responseContent);
                _logger.LogError("Mono Credit History API Error: {Message}", errorResponse?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse Mono credit history error response");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit history from Mono: {Message}", ex.Message);
            return null;
        }
    }

    public async Task<MonoCreditAnalysisResultDto?> AnalyzeCreditHistoryAsync(string bvn, string provider = "xds", string? userId = null)
    {
        try
        {
            _logger.LogInformation("Starting credit analysis for BVN: {BvnMasked} with provider: {Provider}",
                MaskBvn(bvn), provider);

            // Get credit history from Mono
            var creditHistoryResponse = await GetCreditHistoryAsync(bvn, provider, userId);

            if (creditHistoryResponse?.Data == null || creditHistoryResponse.Status != "successful")
            {
                _logger.LogWarning("Failed to retrieve credit history for analysis");
                return null;
            }

            // Perform credit analysis
            var analysis = PerformCreditAnalysis(creditHistoryResponse.Data, bvn, provider);

            _logger.LogInformation("Credit analysis completed for BVN: {BvnMasked} - Score: {Score}, Risk: {Risk}, Action: {Action}",
                MaskBvn(bvn), analysis.CreditScore, analysis.RiskLevel, analysis.RecommendedAction);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing credit history: {Message}", ex.Message);
            return null;
        }
    }

    // Private Helper Methods
    private static string GenerateBvnHash(string bvn)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(bvn));
        return Convert.ToBase64String(hash);
    }

    private static string MaskBvn(string bvn)
    {
        if (string.IsNullOrEmpty(bvn) || bvn.Length < 4)
            return "****";

        return $"{bvn[..3]}***{bvn[^1]}";
    }

    private MonoCreditAnalysisResultDto PerformCreditAnalysis(MonoCreditHistoryDataDto creditData, string bvn, string provider)
    {
        var analysis = new MonoCreditAnalysisResultDto
        {
            Bvn = MaskBvn(bvn),
            Provider = provider,
            AnalyzedAt = DateTime.UtcNow
        };

        var riskFactors = new List<string>();
        var positiveFactors = new List<string>();

        // Analyze credit history
        var allAccounts = creditData.CreditHistory.SelectMany(ch => ch.History).ToList();
        analysis.ActiveLoansCount = allAccounts.Count(a => a.LoanStatus?.ToLower() == "open");
        analysis.TotalOutstandingDebt = allAccounts.Where(a => a.LoanStatus?.ToLower() == "open").Sum(a => a.RepaymentAmount);

        // Calculate performance metrics
        var performingAccounts = allAccounts.Count(a => a.PerformanceStatus?.ToLower() == "performing");
        var totalAccounts = allAccounts.Count;
        var performanceRatio = totalAccounts > 0 ? (decimal)performingAccounts / totalAccounts : 0;

        // Determine overall performance status
        if (performanceRatio >= 0.8m)
        {
            analysis.OverallPerformanceStatus = "excellent";
            positiveFactors.Add("Strong repayment history");
        }
        else if (performanceRatio >= 0.6m)
        {
            analysis.OverallPerformanceStatus = "good";
            positiveFactors.Add("Acceptable repayment history");
        }
        else if (performanceRatio >= 0.4m)
        {
            analysis.OverallPerformanceStatus = "fair";
            riskFactors.Add("Mixed repayment performance");
        }
        else
        {
            analysis.OverallPerformanceStatus = "poor";
            riskFactors.Add("Poor repayment history");
        }

        // Calculate credit score (0-1000 scale)
        var baseScore = 300m; // Minimum score
        var performanceScore = performanceRatio * 400m; // Up to 400 points for performance
        var diversityScore = Math.Min(creditData.CreditHistory.Count * 50m, 200m); // Up to 200 points for credit diversity
        var experienceScore = Math.Min(totalAccounts * 25m, 100m); // Up to 100 points for credit experience

        analysis.CreditScore = Math.Min(baseScore + performanceScore + diversityScore + experienceScore, 1000m);

        // Determine risk level
        if (analysis.CreditScore >= 750m)
        {
            analysis.RiskLevel = "Low";
            analysis.MaxLoanAmount = 5000000m; // 5M NGN for excellent credit
        }
        else if (analysis.CreditScore >= 600m)
        {
            analysis.RiskLevel = "Medium";
            analysis.MaxLoanAmount = 2000000m; // 2M NGN for good credit
        }
        else
        {
            analysis.RiskLevel = "High";
            analysis.MaxLoanAmount = 500000m; // 500K NGN for poor credit
        }

        // Additional risk factors
        if (analysis.ActiveLoansCount > 3)
        {
            riskFactors.Add("Multiple active loans");
        }

        if (analysis.TotalOutstandingDebt > 10000000m) // 10M NGN
        {
            riskFactors.Add("High outstanding debt");
        }

        // Positive factors
        if (analysis.ActiveLoansCount == 0)
        {
            positiveFactors.Add("No current outstanding debt");
        }

        if (creditData.CreditHistory.Count > 2)
        {
            positiveFactors.Add("Good banking relationship diversity");
        }

        // Recommended action
        if (analysis.CreditScore >= 700m && analysis.RiskLevel == "Low")
        {
            analysis.RecommendedAction = "Approve";
        }
        else if (analysis.CreditScore >= 500m && riskFactors.Count <= 2)
        {
            analysis.RecommendedAction = "Review";
        }
        else
        {
            analysis.RecommendedAction = "Decline";
        }

        analysis.RiskFactors = riskFactors;
        analysis.PositiveFactors = positiveFactors;

        return analysis;
    }

    public async Task<MonoCreditworthinessResponseDto?> CheckCreditworthinessAsync(MonoCreditworthinessRequestDto request, string? userId = null)
    {
        try
        {
            // Build the request payload
            var payload = new
            {
                bvn = request.Bvn,
                principal = request.Principal,
                interest_rate = request.InterestRate,
                term = request.Term,
                run_credit_check = request.RunCreditCheck,
                existing_loans = request.ExistingLoans
            };

            // Create HTTP request
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/v2/accounts/id/creditworthiness"))
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            // Add headers
            AddMonoHeaders(httpRequest);

            _logger.LogInformation("Sending Mono creditworthiness check request for BVN: {BVN}", request.Bvn);

            // Send request
            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Mono creditworthiness check failed. Status: {Status}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }

            var result = JsonSerializer.Deserialize<MonoCreditworthinessResponseDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Mono creditworthiness check initiated successfully for BVN: {BVN}", request.Bvn);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking creditworthiness for BVN: {BVN}", request.Bvn);
            return null;
        }
    }

    private void AddMonoHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("mono-sec-key", _settings.SecretKey);
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogError("BaseUrl is not configured in MonoSettings");
            throw new InvalidOperationException("Mono BaseUrl is not configured");
        }

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{path}";
        _logger.LogDebug("Built Mono URL: {Url}", url);
        return url;
    }

    private async Task SaveMandateReferenceAsync(Guid companyId, Guid loanId, MonoMandateDataDto mandateData)
    {
        try
        {
            var mandateReference = new MonoMandateReference
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                LoanId = loanId,
                MandateId = mandateData.Id,
                Reference = mandateData.Reference,
                NibssCode = mandateData.NibssCode,
                Status = mandateData.Status,
                MandateType = mandateData.MandateType,
                DebitType = mandateData.DebitType,
                ReadyToDebit = mandateData.ReadyToDebit,
                Approved = mandateData.Approved,
                AccountName = mandateData.AccountName,
                AccountNumber = mandateData.AccountNumber,
                Bank = mandateData.Bank,
                BankCode = mandateData.BankCode,
                Customer = mandateData.Customer,
                FeeBearer = mandateData.FeeBearer,
                Description = mandateData.Description,
                LiveMode = mandateData.LiveMode,
                StartDate = mandateData.StartDate,
                EndDate = mandateData.EndDate,
                InitialDebitDate = mandateData.InitialDebitDate,
                Amount = mandateData.Amount,
                InitialDebitAmount = mandateData.InitialDebitAmount,
                TransferDestinationsJson = JsonSerializer.Serialize(mandateData.TransferDestinations),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.MonoMandateReferences.Add(mandateReference);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Saved Mono mandate reference to database: {MandateId} for company: {CompanyId}",
                mandateData.Id, companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Mono mandate reference to database for mandate: {MandateId}",
                mandateData.Id);
            // Don't throw - this shouldn't fail the mandate creation
        }
    }
}