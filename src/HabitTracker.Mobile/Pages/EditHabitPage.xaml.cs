using HabitTracker.Mobile.ViewModels;

namespace HabitTracker.Mobile.Pages;

public partial class EditHabitPage : ContentPage
{
    private readonly EditHabitViewModel _vm;

    public EditHabitPage(EditHabitViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Load();
    }
}
