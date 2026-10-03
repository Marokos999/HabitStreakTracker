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

    private bool _hasLoaded;
    private bool _loadFailed;
    private string? _loadError;

    public bool IsEmpty => ActiveHabits.Count == 0 && CompletedHabits.Count == 0;

    // The empty state only makes sense once the first load has finished successfully.
    public bool ShowEmptyState => _hasLoaded && !_loadFailed && IsEmpty;
    public bool ShowLoadError => _loadFailed && IsEmpty;
    public bool ShowInitialLoading => IsBusy && !_hasLoaded && IsEmpty;
    public string? LoadError => _loadError;

    public string TodayText => DateTime.Today.ToString("dddd, MMMM d");

    public ICommand LoadCommand => new Command(async () => await LoadAsync());
    public ICommand NavigateToAddCommand => new Command(async () =>
        await Shell.Current.GoToAsync(nameof(AddHabitPage)));

    public async Task LoadAsync()
    {
        IsBusy = true;
        NotifyListChanged();
        try
        {
            var habits = await habitService.GetHabitsAsync();

            // The server tells which habits are already checked in today, so this survives app restarts
            ActiveHabits.Clear();
            CompletedHabits.Clear();

            foreach (var h in habits)
            {
                if (h.CheckedInToday)
                    CompletedHabits.Add(h);
                else
                    ActiveHabits.Add(h);
            }

            _loadFailed = false;
            _loadError = null;
            HasDoneSection = CompletedHabits.Count > 0;
        }
        catch (ApiException ex)
        {
            _loadFailed = true;
            _loadError = ex.Message;
            if (!IsEmpty)
                await ErrorAlert.ShowAsync(ex); // keep showing the old list
        }
        finally
        {
            _hasLoaded = true;
            IsBusy = false;
            NotifyListChanged();
        }
    }

    public async Task NavigateToDetailAsync(Habit habit)
    {
        NavigationState.SelectedHabit = habit;
        await Shell.Current.GoToAsync($"{nameof(HabitDetailPage)}?habitId={habit.Id}");
    }

    // onSuccess runs after the server accepted the check-in and before the card moves (used for animation).
    public async Task CheckInHabitAsync(Habit habit, Func<Task>? onSuccess = null)
    {
        try
        {
            await habitService.CheckInAsync(habit.Id);

            if (onSuccess is not null)
                await onSuccess();

            ActiveHabits.Remove(habit);
            habit.CheckedInToday = true;
            if (!CompletedHabits.Any(h => h.Id == habit.Id))
                CompletedHabits.Add(habit);
            HasDoneSection = CompletedHabits.Count > 0;
            NotifyListChanged();
        }
        catch (ApiException ex)
        {
            await ErrorAlert.ShowAsync(ex);
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
            NotifyListChanged();
        }
        catch (ApiException ex)
        {
            await ErrorAlert.ShowAsync(ex);
        }
    }

    private void NotifyListChanged()
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(ShowEmptyState));
        OnPropertyChanged(nameof(ShowLoadError));
        OnPropertyChanged(nameof(ShowInitialLoading));
        OnPropertyChanged(nameof(LoadError));
    }
}
