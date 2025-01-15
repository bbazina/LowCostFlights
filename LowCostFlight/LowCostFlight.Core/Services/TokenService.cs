using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LowCostFlight.Core.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private string _accessToken;
        private DateTime _accessTokenExpiry;

        public TokenService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(_accessToken) || DateTime.UtcNow >= _accessTokenExpiry)
            {
                await _semaphore.WaitAsync();
                // double check locking
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow <= _accessTokenExpiry)
                {
                    _semaphore.Release();
                    return _accessToken;
                }
                try
                {
                    var url = _configuration.GetSection("AmadeusApi:ApiAccessTokenUrl").Value;
                    var clientId = _configuration.GetSection("AmadeusApi:ApiKey").Value;
                    var clientSecret = _configuration.GetSection("AmadeusApi:ApiSecret").Value;
                    var request = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "client_credentials"),
                            new KeyValuePair<string, string>("client_id", clientId),
                            new KeyValuePair<string, string>("client_secret", clientSecret)
                        })

                    };

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var content = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

                    _accessToken = tokenResponse.GetProperty("access_token").GetString();
                    _accessTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetDouble());
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            return _accessToken;
        }
    }
}
