using HabitTracker.Mobile.ViewModels;

namespace HabitTracker.Mobile.Pages;

public partial class HabitDetailPage : ContentPage
{
    public HabitDetailPage(HabitDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
