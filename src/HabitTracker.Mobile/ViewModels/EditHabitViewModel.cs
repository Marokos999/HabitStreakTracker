using System.Windows.Input;
using HabitTracker.Mobile.Models;
using HabitTracker.Mobile.Services;

namespace HabitTracker.Mobile.ViewModels;

public class EditHabitViewModel(IHabitService habitService) : BaseViewModel
{
    private Habit? _habit;
    private string _name = string.Empty;
    private string? _description;
    private string _color = "#6366F1";
    private int _targetDaysPerWeek = 7;

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public string? Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(); }
    }

    public int TargetDaysPerWeek
    {
        get => _targetDaysPerWeek;
        set { _targetDaysPerWeek = value; OnPropertyChanged(); }
    }

    public List<string> PresetColors { get; } =
    [
        "#6366F1", "#EF4444", "#10B981", "#F59E0B",
        "#3B82F6", "#8B5CF6", "#EC4899", "#14B8A6"
    ];

    public ICommand SelectColorCommand => new Command<string>(color => Color = color);
    public ICommand SaveCommand => new Command(async () => await SaveAsync());

    public void Load()
    {
        _habit = NavigationState.SelectedHabit;
        if (_habit is null) return;
        Name = _habit.Name;
        Description = _habit.Description;
        Color = _habit.Color ?? "#6366F1";
        TargetDaysPerWeek = _habit.TargetDaysPerWeek;
    }

    private async Task SaveAsync()
    {
        if (_habit is null || string.IsNullOrWhiteSpace(Name)) return;
        IsBusy = true;
        try
        {
            await habitService.UpdateHabitAsync(_habit.Id, Name, Description, Color, TargetDaysPerWeek);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }
}
