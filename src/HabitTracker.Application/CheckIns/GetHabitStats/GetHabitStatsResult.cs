using HabitTracker.Domain.ValueObjects;

namespace HabitTracker.Application.CheckIns.GetHabitStats;

// RecentCheckInDates covers the last GetHabitStatsHandler.RecentDays days (used for the heatmap).
public record GetHabitStatsResult(StreakResult Streak, IReadOnlyList<DateOnly> RecentCheckInDates);
