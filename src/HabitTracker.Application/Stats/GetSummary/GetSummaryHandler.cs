using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Services;

namespace HabitTracker.Application.Stats.GetSummary;

public class GetSummaryHandler(IHabitRepository habitRepo, ICheckInRepository checkInRepo)
{
    public async Task<GetSummaryResult> Handle(GetSummaryQuery query)
    {
        var habits = await habitRepo.GetAllAsync(query.UserId);
        if (habits.Count == 0)
            return new GetSummaryResult(0, 0, 0, 0);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int checkedInToday = 0;
        int bestStreak = 0;
        double totalCompletion = 0;

        foreach (var habit in habits)
        {
            var checkIns = await checkInRepo.GetByHabitAsync(query.UserId, habit.Id);
            var dates = checkIns.Select(c => c.Date).ToList();
            if (dates.Contains(today)) checkedInToday++;
            var streak = StreakCalculator.Calculate(dates, today, habit.TargetDaysPerWeek);
            if (streak.CurrentStreak > bestStreak) bestStreak = streak.CurrentStreak;
            totalCompletion += streak.CompletionRate;
        }

        return new GetSummaryResult(
            TotalHabits: habits.Count,
            CheckedInToday: checkedInToday,
            BestCurrentStreak: bestStreak,
            AverageCompletionRate: totalCompletion / habits.Count);
    }
}
