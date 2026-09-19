namespace HabitTracker.Mobile.Services;

public class SecureStorageService
{
    private const string AccessTokenKey = "access_token";

    public async Task<string?> GetAccessTokenAsync() =>
        await SecureStorage.Default.GetAsync(AccessTokenKey);

    public async Task SetAccessTokenAsync(string token) =>
        await SecureStorage.Default.SetAsync(AccessTokenKey, token);

    public void ClearAccessToken() =>
        SecureStorage.Default.Remove(AccessTokenKey);
}
