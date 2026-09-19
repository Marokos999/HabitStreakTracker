using System.Windows.Input;
using HabitTracker.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Mobile.ViewModels;

public class LoginViewModel(ICognitoAuthService authService, IServiceProvider serviceProvider) : BaseViewModel
{
    private string? _errorMessage;

    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    public ICommand SignInCommand => new Command(async () => await SignInAsync());
    public ICommand SignUpCommand => new Command(async () => await authService.SignUpAsync());

    private async Task SignInAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var token = await authService.SignInAsync();
            if (token is not null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.Windows[0].Page = serviceProvider.GetRequiredService<AppShell>());
            }
            else
            {
                ErrorMessage = "Sign in failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally { IsBusy = false; }
    }
}
