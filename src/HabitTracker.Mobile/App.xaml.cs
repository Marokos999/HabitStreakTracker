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
            Windows[0].Page = _serviceProvider.GetRequiredService<LoginPage>();
    }
}
