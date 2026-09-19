using HabitTracker.Mobile.ViewModels;

namespace HabitTracker.Mobile.Pages;

public partial class StatsPage : ContentPage
{
    private readonly StatsViewModel _vm;

    public StatsPage(StatsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
