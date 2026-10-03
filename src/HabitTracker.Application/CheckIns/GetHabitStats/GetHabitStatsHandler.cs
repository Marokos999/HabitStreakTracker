using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Services;

namespace HabitTracker.Application.CheckIns.GetHabitStats;

public class GetHabitStatsHandler(ICheckInRepository repo)
{
    // 12 weeks, enough for a GitHub-style heatmap
    public const int RecentDays = 84;

    public async Task<GetHabitStatsResult> Handle(GetHabitStatsQuery query)
    {
        var checkIns = await repo.GetByHabitAsync(query.UserId, query.HabitId);
        var dates = checkIns.Select(c => c.Date).Distinct().ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = StreakCalculator.Calculate(dates, today);

        var windowStart = today.AddDays(-(RecentDays - 1));
        var recent = dates
            .Where(d => d >= windowStart && d <= today)
            .OrderBy(d => d)
            .ToList();

        return new GetHabitStatsResult(streak, recent);
    }
}
