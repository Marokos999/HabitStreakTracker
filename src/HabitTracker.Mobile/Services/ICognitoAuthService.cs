namespace HabitTracker.Mobile.Services;

public interface ICognitoAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetAccessTokenAsync();
    Task<string?> SignInAsync();
    Task<string?> SignUpAsync();
    Task SignOutAsync();
}
