namespace HabitTracker.Mobile.Models;

public class SummaryResult
{
    public int TotalHabits { get; set; }
    public int CheckedInToday { get; set; }
    public int BestCurrentStreak { get; set; }
    public double AverageCompletionRate { get; set; }
}
