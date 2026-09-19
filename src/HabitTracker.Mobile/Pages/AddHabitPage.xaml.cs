using HabitTracker.Mobile.ViewModels;

namespace HabitTracker.Mobile.Pages;

public partial class AddHabitPage : ContentPage
{
    public AddHabitPage(AddHabitViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
