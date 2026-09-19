using System.Text.Json;

namespace HabitTracker.Mobile.Services;

public class CognitoAuthService(SecureStorageService storage) : ICognitoAuthService
{
    // Fill these in after SAM deploy
    private const string Domain = "YOUR_COGNITO_DOMAIN.auth.eu-central-1.amazoncognito.com";
    private const string ClientId = "YOUR_CLIENT_ID";
    private const string CallbackUrl = "habittracker://callback";

    public async Task<bool> IsAuthenticatedAsync() =>
        await storage.GetAccessTokenAsync() is not null;

    public async Task<string?> GetAccessTokenAsync() =>
        await storage.GetAccessTokenAsync();

    public async Task SignUpAsync()
    {
        var signUpUrl = $"https://{Domain}/signup?response_type=code&client_id={ClientId}" +
                        $"&redirect_uri={Uri.EscapeDataString(CallbackUrl)}&scope=openid+email";
        await Browser.Default.OpenAsync(signUpUrl, BrowserLaunchMode.SystemPreferred);
    }

    public async Task<string?> SignInAsync()
    {
        try
        {
            var authUrl = $"https://{Domain}/login?response_type=code&client_id={ClientId}" +
                          $"&redirect_uri={Uri.EscapeDataString(CallbackUrl)}&scope=openid+email";

            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authUrl), new Uri(CallbackUrl));

            var code = result.Properties.GetValueOrDefault("code");
            if (code is null) return null;

            var token = await ExchangeCodeAsync(code);
            if (token is not null)
                await storage.SetAccessTokenAsync(token);

            return token;
        }
        catch
        {
            return null;
        }
    }

    public async Task SignOutAsync()
    {
        storage.ClearAccessToken();
        var logoutUrl = $"https://{Domain}/logout?client_id={ClientId}" +
                        $"&logout_uri={Uri.EscapeDataString(CallbackUrl)}";
        await Launcher.Default.OpenAsync(logoutUrl);
    }

    private async Task<string?> ExchangeCodeAsync(string code)
    {
        using var http = new HttpClient();
        var response = await http.PostAsync($"https://{Domain}/oauth2/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = ClientId,
                ["code"] = code,
                ["redirect_uri"] = CallbackUrl
            }));

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("access_token", out var token)
            ? token.GetString()
            : null;
    }
}
