namespace HabitTracker.Mobile.Services;

public static class ErrorAlert
{
    public static async Task ShowAsync(Exception ex)
    {
        // An expired session sends the user back to the sign-in screen, no alert needed
        if (ex is ApiException { IsUnauthorized: true })
            return;

        if (Shell.Current is { } shell)
            await shell.DisplayAlertAsync("Error", ex.Message, "OK");
    }
}
