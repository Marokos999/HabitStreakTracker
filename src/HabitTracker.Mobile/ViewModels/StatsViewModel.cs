using System.Windows.Input;
using HabitTracker.Mobile.Models;
using HabitTracker.Mobile.Services;

namespace HabitTracker.Mobile.ViewModels;

public class StatsViewModel(IHabitService habitService) : BaseViewModel
{
    private SummaryResult? _summary;
    private bool _hasError;

    public int TotalHabits => _summary?.TotalHabits ?? 0;
    public int CheckedInToday => _summary?.CheckedInToday ?? 0;
    public int BestCurrentStreak => _summary?.BestCurrentStreak ?? 0;
    public string AverageCompletion => _summary is null ? "–" : $"{_summary.AverageCompletionRate:F0}%";
    public string TodayProgress => TotalHabits == 0 ? "Add your first habit" :
        $"{CheckedInToday} of {TotalHabits} done today";
    public double CompletionProgress => TotalHabits == 0 ? 0 :
        (double)CheckedInToday / TotalHabits;
    public bool HasError { get => _hasError; private set { _hasError = value; OnPropertyChanged(); } }

    public ICommand LoadCommand => new Command(async () => await LoadAsync());

    public async Task LoadAsync()
    {
        IsBusy = true;
        HasError = false;
        try
        {
            _summary = await habitService.GetSummaryAsync();
            OnPropertyChanged(nameof(TotalHabits));
            OnPropertyChanged(nameof(CheckedInToday));
            OnPropertyChanged(nameof(BestCurrentStreak));
            OnPropertyChanged(nameof(AverageCompletion));
            OnPropertyChanged(nameof(TodayProgress));
            OnPropertyChanged(nameof(CompletionProgress));
        }
        catch (ApiException)
        {
            HasError = true;
        }
        finally { IsBusy = false; }
    }
}
