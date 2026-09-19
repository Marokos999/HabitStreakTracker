using HabitTracker.Mobile.Models;
using HabitTracker.Mobile.ViewModels;

namespace HabitTracker.Mobile.Pages;

public partial class HabitsPage : ContentPage
{
    private readonly HabitsViewModel _vm;

    public HabitsPage(HabitsViewModel vm)
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

    private async void OnHabitTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement el && el.BindingContext is Habit habit)
            await _vm.NavigateToDetailAsync(habit);
    }

    private async void OnCheckInClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Habit habit)
            await _vm.CheckInHabitAsync(habit);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Habit habit)
            await _vm.DeleteHabitAsync(habit);
    }
}
