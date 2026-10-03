using HabitTracker.Mobile.Pages;
using HabitTracker.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Mobile;

public partial class App : Application
{
    private readonly ICognitoAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public App(ICognitoAuthService authService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _serviceProvider = serviceProvider;
        _authService.SessionExpired += OnSessionExpired;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start with AppShell; OnStart redirects to LoginPage if not authenticated
        return new Window(_serviceProvider.GetRequiredService<AppShell>());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        var isAuthenticated = await _authService.IsAuthenticatedAsync();
        if (!isAuthenticated)
            ShowLogin();
    }

    // Token rejected or refresh failed: go back to the sign-in screen
    private void OnSessionExpired() => MainThread.BeginInvokeOnMainThread(ShowLogin);

    private void ShowLogin()
    {
        if (Windows.Count == 0 || Windows[0].Page is LoginPage)
            return; // already on the sign-in screen (several requests can fail at once)

        Windows[0].Page = _serviceProvider.GetRequiredService<LoginPage>();
    }
}
