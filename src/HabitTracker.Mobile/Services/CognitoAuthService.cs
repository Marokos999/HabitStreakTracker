using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace HabitTracker.Mobile.Services;

public class CognitoAuthService(SecureStorageService storage, CognitoSettings settings) : ICognitoAuthService
{
    private const string Scope = "openid email";
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(1);

    private readonly HttpClient _http = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public async Task<bool> IsAuthenticatedAsync()
    {
        var tokens = await storage.GetTokensAsync();
        return tokens is not null && (tokens.RefreshToken is not null || tokens.ExpiresAt > DateTimeOffset.UtcNow);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var tokens = await storage.GetTokensAsync();
        if (tokens is null)
            return null;

        if (tokens.ExpiresAt - RefreshSkew > DateTimeOffset.UtcNow)
            return tokens.AccessToken;

        return await RefreshAsync();
    }

    public Task<string?> SignInAsync() => AuthenticateAsync("login");

    // The hosted UI sign-up page returns an authorization code once the user has confirmed the account.
    public Task<string?> SignUpAsync() => AuthenticateAsync("signup");

    public async Task SignOutAsync()
    {
        storage.ClearTokens();
        var logoutUrl = $"https://{settings.Domain}/logout?client_id={settings.ClientId}" +
                        $"&logout_uri={Uri.EscapeDataString(settings.CallbackUrl)}";
        await Launcher.Default.OpenAsync(logoutUrl);
    }

    private async Task<string?> AuthenticateAsync(string path)
    {
        EnsureConfigured();

        var verifier = Pkce.CreateVerifier();
        var state = Pkce.CreateState();
        var authUrl = $"https://{settings.Domain}/{path}?response_type=code&client_id={settings.ClientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(settings.CallbackUrl)}" +
                      $"&scope={Uri.EscapeDataString(Scope)}&state={state}" +
                      $"&code_challenge={Pkce.CreateChallenge(verifier)}&code_challenge_method=S256";

        WebAuthenticatorResult result;
        try
        {
            result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = new Uri(authUrl),
                    CallbackUrl = new Uri(settings.CallbackUrl)
                });
        }
        catch (TaskCanceledException)
        {
            return null; // user closed the browser
        }

        if (result.Properties.GetValueOrDefault("state") != state)
            throw new InvalidOperationException("Sign in failed: state mismatch.");

        var code = result.Properties.GetValueOrDefault("code");
        if (code is null)
            return null;

        var tokens = await RequestTokensAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = settings.ClientId,
            ["code"] = code,
            ["redirect_uri"] = settings.CallbackUrl,
            ["code_verifier"] = verifier
        }, existingRefreshToken: null);

        await storage.SetTokensAsync(tokens);
        return tokens.AccessToken;
    }

    private async Task<string?> RefreshAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            // Another caller may have refreshed while this one was waiting for the lock.
            var current = await storage.GetTokensAsync();
            if (current is null)
                return null;
            if (current.ExpiresAt - RefreshSkew > DateTimeOffset.UtcNow)
                return current.AccessToken;
            if (current.RefreshToken is null)
            {
                storage.ClearTokens();
                return null;
            }

            try
            {
                var tokens = await RequestTokensAsync(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = settings.ClientId,
                    ["refresh_token"] = current.RefreshToken
                }, current.RefreshToken);

                await storage.SetTokensAsync(tokens);
                return tokens.AccessToken;
            }
            catch (HttpRequestException)
            {
                // Refresh token expired or revoked: the user has to sign in again.
                storage.ClearTokens();
                return null;
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<AuthTokens> RequestTokensAsync(Dictionary<string, string> form, string? existingRefreshToken)
    {
        using var response = await _http.PostAsync($"https://{settings.Domain}/oauth2/token",
            new FormUrlEncodedContent(form));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Token request failed ({(int)response.StatusCode}): {error}");
        }

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>()
                    ?? throw new HttpRequestException("Token response was empty.");

        return new AuthTokens(
            token.AccessToken,
            token.RefreshToken ?? existingRefreshToken,
            DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn));
    }

    private void EnsureConfigured()
    {
        if (!settings.IsConfigured)
            throw new InvalidOperationException(
                $"Cognito is not configured. Set domain and clientId in Resources/Raw/{CognitoSettings.FileName}.");
    }

    private record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string? RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
