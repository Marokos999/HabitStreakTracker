using System.Windows.Input;
using HabitTracker.Mobile.Models;
using HabitTracker.Mobile.Pages;
using HabitTracker.Mobile.Services;

namespace HabitTracker.Mobile.ViewModels;

[QueryProperty(nameof(Ignored), "habitId")]
public class HabitDetailViewModel(IHabitService habitService) : BaseViewModel
{
    private Habit? _habit;
    private StreakResult? _streak;

    public string Ignored
    {
        set
        {
            _habit = NavigationState.SelectedHabit;
            OnPropertyChanged(nameof(HabitName));
            OnPropertyChanged(nameof(HabitColor));
            if (_habit is not null)
                _ = LoadAsync();
        }
    }

    public string HabitName => _habit?.Name ?? string.Empty;
    public string HabitColor => _habit?.Color ?? "#6366F1";

    public StreakResult? Streak
    {
        get => _streak;
        set { _streak = value; OnPropertyChanged(); }
    }

    public ICommand CheckInCommand => new Command(async () => await CheckInAsync());
    public ICommand NavigateToEditCommand => new Command(async () =>
        await Shell.Current.GoToAsync(nameof(EditHabitPage)));

    public async Task LoadAsync()
    {
        if (_habit is null) return;
        IsBusy = true;
        try { Streak = await habitService.GetStatsAsync(_habit.Id); }
        catch (ApiException ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }

    private async Task CheckInAsync()
    {
        if (_habit is null) return;
        try
        {
            await habitService.CheckInAsync(_habit.Id);
            await LoadAsync();
            await Shell.Current.DisplayAlertAsync("✓ Done", $"{HabitName} checked in for today!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}
