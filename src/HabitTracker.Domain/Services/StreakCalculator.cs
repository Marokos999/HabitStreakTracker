using HabitTracker.Domain.ValueObjects;

namespace HabitTracker.Domain.Services;

public static class StreakCalculator
{
  public static StreakResult Calculate(IEnumerable<DateOnly> dates, DateOnly today)
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

    var completionRate = Math.Min(sorted.Count / 30.0 * 100, 100);
    return new StreakResult(current, longest, sorted.Count, completionRate);
  }
}
