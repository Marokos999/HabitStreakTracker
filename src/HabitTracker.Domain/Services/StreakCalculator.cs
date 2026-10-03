using HabitTracker.Domain.ValueObjects;

namespace HabitTracker.Domain.Services;

public static class StreakCalculator
{
    public const int CompletionWindowDays = 30;

    // CompletionRate compares the check-ins of the last 30 days with how many the habit's weekly target
    // asks for in that window (targetDaysPerWeek * 30 / 7), so a 3x-per-week habit can reach 100%.
    public static StreakResult Calculate(IEnumerable<DateOnly> dates, DateOnly today, int targetDaysPerWeek = 7)
    {
        var sorted = dates.Distinct().OrderByDescending(d => d).ToList();
        if (sorted.Count == 0)
            return new StreakResult(0, 0, 0);

        int longest = 0, run = 0, current = 0;
        var currentClosed = false;
        DateOnly? prev = null;

        foreach (var date in sorted)
        {
            if (prev is null || prev.Value.AddDays(-1) == date)
                run++;
            else
            {
                currentClosed = true;
                run = 1;
            }

            if (!currentClosed)
                current = run;

            prev = date;
            longest = Math.Max(longest, run);
        }

        if (sorted[0] < today.AddDays(-1))
            current = 0;

        return new StreakResult(current, longest, sorted.Count, CompletionRate(sorted, today, targetDaysPerWeek));
    }

    private static double CompletionRate(List<DateOnly> dates, DateOnly today, int targetDaysPerWeek)
    {
        var windowStart = today.AddDays(-(CompletionWindowDays - 1));
        var inWindow = dates.Count(d => d >= windowStart && d <= today);
        var expected = Math.Clamp(targetDaysPerWeek, 1, 7) * CompletionWindowDays / 7.0;

        return Math.Min(inWindow / expected * 100, 100);
    }
}
