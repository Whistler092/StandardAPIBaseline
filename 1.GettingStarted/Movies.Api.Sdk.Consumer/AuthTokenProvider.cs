using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;

namespace Movies.Api.Sdk.Consumer;

public class AuthTokenProvider
{
    private readonly HttpClient _httpClient;
    private string _cacheToken = string.Empty;
    private static readonly SemaphoreSlim Lock = new(1, 1);

    public AuthTokenProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cacheToken))
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_cacheToken);
            var expireTimeText = jwt.Claims.Single(claim => claim.Type == "exp").Value;
            var expireTime = UnixTimeStampToDateTime(double.Parse(expireTimeText));
            if (expireTime > DateTime.UtcNow)
            {
                return _cacheToken;
            }
        }

        await Lock.WaitAsync();
        var response = await _httpClient.PostAsJsonAsync("https://localhost:5003/token", new
        {
            userid = "d8566de3-b1a6-4a9b-b842-8e3887a82e41",
            email = "nick@nickchapsas.com",
            customClaims = new Dictionary<string, object>
            {
                { "admin", true },
                { "trusted_member", true}
            }
        });

        var newToken = await response.Content.ReadAsStringAsync();
        _cacheToken = newToken;
        Lock.Release();

        return newToken;
    }

    private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dateTime;
    }
}
