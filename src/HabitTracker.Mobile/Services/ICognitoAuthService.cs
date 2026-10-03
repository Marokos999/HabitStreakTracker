namespace HabitTracker.Mobile.Services;

public interface ICognitoAuthService
{
    // Raised when the stored session can no longer be used (token rejected or refresh failed)
    event Action? SessionExpired;

    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetAccessTokenAsync();
    Task<string?> SignInAsync();
    Task<string?> SignUpAsync();
    Task SignOutAsync();

    // Called when the API rejects the access token: clears the session and raises SessionExpired
    Task HandleUnauthorizedAsync();
}
