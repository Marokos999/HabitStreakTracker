namespace HabitTracker.Mobile.Models;

public class StreakResult
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCheckIns { get; set; }
    public double CompletionRate { get; set; }
}
