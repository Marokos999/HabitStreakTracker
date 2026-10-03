using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.Mobile.ViewModels;

public class BaseViewModel : INotifyPropertyChanged
{
    private bool _isBusy;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotBusy));
        }
    }

    // For disabling buttons while a request is running
    public bool IsNotBusy => !_isBusy;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
