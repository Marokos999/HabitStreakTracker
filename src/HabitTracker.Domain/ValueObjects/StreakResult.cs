namespace HabitTracker.Domain.ValueObjects;

public record StreakResult
(
  int CurrentStreak,
  int LongestStreak,
  int TotalCheckIns,
  double CompletionRate = 0
);
