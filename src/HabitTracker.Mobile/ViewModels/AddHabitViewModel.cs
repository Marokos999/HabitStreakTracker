using System.Windows.Input;
using HabitTracker.Mobile.Services;

namespace HabitTracker.Mobile.ViewModels;

public class AddHabitViewModel(IHabitService habitService) : BaseViewModel
{
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

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;
        IsBusy = true;
        try
        {
            await habitService.CreateHabitAsync(Name, Description, Color, TargetDaysPerWeek);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally { IsBusy = false; }
    }
}
