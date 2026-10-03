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

    private async void OnHabitTapped(object? sender, TappedEventArgs e)
    {
        if (sender is VisualElement el && el.BindingContext is Habit habit)
            await _vm.NavigateToDetailAsync(habit);
    }

    private async void OnCheckInClicked(object? sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Habit habit)
            return;

        var card = FindParent<Border>(btn);
        btn.IsEnabled = false; // ignore double taps while the request and animation run
        try
        {
            await _vm.CheckInHabitAsync(habit, () => PlayCheckInAnimationAsync(btn, card));
        }
        finally
        {
            btn.IsEnabled = true;
        }
    }

    // Button pops, then the card slides out and fades before it moves to "Done Today".
    private static async Task PlayCheckInAnimationAsync(Button button, Border? card)
    {
        await button.ScaleToAsync(1.3, 90, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 90, Easing.CubicIn);

        if (card is null)
            return;

        await Task.WhenAll(
            card.FadeToAsync(0, 220, Easing.CubicIn),
            card.TranslateToAsync(48, 0, 220, Easing.CubicIn));
    }

    private static T? FindParent<T>(Element element) where T : Element
    {
        for (var parent = element.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is T match)
                return match;
        }

        return null;
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Habit habit)
            await _vm.DeleteHabitAsync(habit);
    }
}
