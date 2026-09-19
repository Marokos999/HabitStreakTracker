using System.Collections.ObjectModel;
using System.Windows.Input;
using HabitTracker.Mobile.Models;
using HabitTracker.Mobile.Pages;
using HabitTracker.Mobile.Services;

namespace HabitTracker.Mobile.ViewModels;

public class HabitsViewModel(IHabitService habitService) : BaseViewModel
{
    public ObservableCollection<Habit> ActiveHabits { get; } = [];
    public ObservableCollection<Habit> CompletedHabits { get; } = [];

    private bool _hasDoneSection;
    public bool HasDoneSection
    {
        get => _hasDoneSection;
        private set { _hasDoneSection = value; OnPropertyChanged(); }
    }

    public bool IsEmpty => ActiveHabits.Count == 0 && CompletedHabits.Count == 0;

    public string TodayText => DateTime.Today.ToString("dddd, MMMM d");

    public ICommand LoadCommand => new Command(async () => await LoadAsync());
    public ICommand NavigateToAddCommand => new Command(async () =>
        await Shell.Current.GoToAsync(nameof(AddHabitPage)));

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var habits = await habitService.GetHabitsAsync();

            // Remember which habits were already checked in this session
            var completedIds = CompletedHabits.Select(h => h.Id).ToHashSet();
            ActiveHabits.Clear();
            CompletedHabits.Clear();

            foreach (var h in habits)
            {
                if (completedIds.Contains(h.Id))
                    CompletedHabits.Add(h);
                else
                    ActiveHabits.Add(h);
            }

            HasDoneSection = CompletedHabits.Count > 0;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (HttpRequestException) { }
        finally { IsBusy = false; }
    }

    public async Task NavigateToDetailAsync(Habit habit)
    {
        NavigationState.SelectedHabit = habit;
        await Shell.Current.GoToAsync($"{nameof(HabitDetailPage)}?habitId={habit.Id}");
    }

    public async Task CheckInHabitAsync(Habit habit)
    {
        try
        {
            await habitService.CheckInAsync(habit.Id);
            ActiveHabits.Remove(habit);
            if (!CompletedHabits.Any(h => h.Id == habit.Id))
                CompletedHabits.Add(habit);
            HasDoneSection = CompletedHabits.Count > 0;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (HttpRequestException)
        {
            await Shell.Current.DisplayAlertAsync("Error", "Could not reach server. Is SAM running?", "OK");
        }
    }

    public async Task DeleteHabitAsync(Habit habit)
    {
        try
        {
            await habitService.DeleteHabitAsync(habit.Id);
            ActiveHabits.Remove(habit);
            CompletedHabits.Remove(habit);
            HasDoneSection = CompletedHabits.Count > 0;
            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (HttpRequestException) { }
    }
}
