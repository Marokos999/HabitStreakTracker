using HabitTracker.Mobile.Pages;

namespace HabitTracker.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(AddHabitPage), typeof(AddHabitPage));
        Routing.RegisterRoute(nameof(HabitDetailPage), typeof(HabitDetailPage));
        Routing.RegisterRoute(nameof(EditHabitPage), typeof(EditHabitPage));
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    }
}
