namespace HabitTracker.Application.Stats.GetSummary;

public record GetSummaryResult(
    int TotalHabits,
    int CheckedInToday,
    int BestCurrentStreak,
    double AverageCompletionRate);
