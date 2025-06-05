using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations
{
    public interface IRemitaService
    {
        string GetUsername();
        string GetPassword();
        Task<string?> GetSalaryHistoryAsync(object requestBody, string apiKey, string merchantId, string requestId, string authorization);
    }

    public class RemitaService : IRemitaService
    {
        private readonly RemitaSettings _settings;
        private readonly HttpClient _httpClient;
        public RemitaService(IOptions<RemitaSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }
        public string GetUsername() => _settings.Username;
        public string GetPassword() => _settings.Password;

        public async Task<string?> GetSalaryHistoryAsync(object requestBody, string apiKey, string merchantId, string requestId, string authorization)
        {
            var url = _settings.BaseUrl.TrimEnd('/') + "/send/api/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("API_KEY", apiKey);
            request.Headers.Add("MERCHANT_ID", merchantId);
            request.Headers.Add("REQUEST_ID", requestId);
            request.Headers.Add("AUTHORIZATION", authorization);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync();
        }
    }
}
