namespace HabitTracker.Mobile.Models;

public class StreakResult
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int TotalCheckIns { get; set; }
    public double CompletionRate { get; set; }

    // Check-ins of the last 12 weeks (oldest first), used for the heatmap
    public List<DateOnly> CheckInDates { get; set; } = [];
}
