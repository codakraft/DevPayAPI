using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations
{
    public interface IRemitaAuthService
    {
        Task<string?> GetAccessTokenAsync();
    }

    public class RemitaAuthService : IRemitaAuthService
    {
        private readonly RemitaSettings _settings;
        private readonly HttpClient _httpClient;
        private string? _cachedToken;
        private DateTime? _tokenExpiry;
        public RemitaAuthService(IOptions<RemitaSettings> options, IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && _tokenExpiry.HasValue && _tokenExpiry > DateTime.UtcNow)
                return _cachedToken;

            var url = _settings.BaseUrl.TrimEnd('/') + "/" + _settings.AuthUrl.TrimStart('/');
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            var credentials = new
            {
                username = _settings.Username,
                password = _settings.Password
            };
            request.Content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("accessToken", out var tokenProp))
            {
                _cachedToken = tokenProp.GetString();
                // Set expiry to 1 hour from now
                _tokenExpiry = DateTime.UtcNow.AddHours(1);
                return _cachedToken;
            }
            return null;
        }
    }
}
