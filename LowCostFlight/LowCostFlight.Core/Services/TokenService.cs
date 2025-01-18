using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LowCostFlight.Core.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _distributedCache;

        public TokenService(
            HttpClient httpClient,
            IConfiguration configuration,
            IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _distributedCache = distributedCache;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            const string redisTokenKey = "amadeus_access_token";

            var cachedToken = await _distributedCache.GetStringAsync(redisTokenKey);

            if (!string.IsNullOrEmpty(cachedToken))
            {
                return cachedToken;
            }

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

            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            var expiresIn = tokenResponse.GetProperty("expires_in").GetDouble();

            if (string.IsNullOrEmpty(accessToken))
            {
                return string.Empty;
            }

            // Subtract a buffer of 10 seconds to avoid expiry during usage
            var expirationTime = TimeSpan.FromSeconds(expiresIn - 10);

            // Store the token in Redis with the expiration time set to the token's expiration
            await _distributedCache.SetStringAsync(redisTokenKey, accessToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expirationTime
            });

            return accessToken;
        }
    }
}
