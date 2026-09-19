using HabitTracker.Mobile.Models;

namespace HabitTracker.Mobile.Services;

public interface IHabitService
{
    Task<List<Habit>> GetHabitsAsync();
    Task<Habit> CreateHabitAsync(string name, string? description, string color, int targetDaysPerWeek);
    Task DeleteHabitAsync(Guid id);
    Task<StreakResult> GetStatsAsync(Guid habitId);
    Task CheckInAsync(Guid habitId);
    Task UpdateHabitAsync(Guid id, string name, string? description, string color, int targetDaysPerWeek);
    Task<SummaryResult> GetSummaryAsync();
}