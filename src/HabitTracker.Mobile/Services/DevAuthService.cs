namespace HabitTracker.Mobile.Services;

// Used in DEBUG mode: bypasses Cognito for local development
public class DevAuthService : ICognitoAuthService
{
    public Task<bool> IsAuthenticatedAsync() => Task.FromResult(true);
    public Task<string?> GetAccessTokenAsync() => Task.FromResult<string?>("dev-local-token");
    public Task<string?> SignInAsync() => Task.FromResult<string?>("dev-local-token");
    public Task<string?> SignUpAsync() => Task.FromResult<string?>("dev-local-token");
    public Task SignOutAsync() => Task.CompletedTask;
}
