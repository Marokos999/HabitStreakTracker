using System.Globalization;

namespace HabitTracker.Mobile.Services;

public record AuthTokens(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt);

public class SecureStorageService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private const string ExpiresAtKey = "access_token_expires_at";

    public async Task<AuthTokens?> GetTokensAsync()
    {
        var access = await SecureStorage.Default.GetAsync(AccessTokenKey);
        if (access is null)
            return null;

        var refresh = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        var expiresRaw = await SecureStorage.Default.GetAsync(ExpiresAtKey);
        var expiresAt = DateTimeOffset.TryParse(expiresRaw, CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                out var parsed)
            ? parsed
            : DateTimeOffset.MinValue;

        return new AuthTokens(access, refresh, expiresAt);
    }

    public async Task SetTokensAsync(AuthTokens tokens)
    {
        await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
        await SecureStorage.Default.SetAsync(ExpiresAtKey, tokens.ExpiresAt.ToString("O", CultureInfo.InvariantCulture));

        if (tokens.RefreshToken is not null)
            await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
    }

    public void ClearTokens()
    {
        SecureStorage.Default.Remove(AccessTokenKey);
        SecureStorage.Default.Remove(RefreshTokenKey);
        SecureStorage.Default.Remove(ExpiresAtKey);
    }
}
