using HabitTracker.Domain.ValueObjects;

namespace HabitTracker.Domain.Services;

public static class StreakCalculator
{
  public static StreakResult Calculate(IEnumerable<DateOnly> dates)
  {
      var sorted = dates.OrderByDescending(d => d).ToList();
      if(sorted.Count == 0)
          return new StreakResult(0,0,0);

      int longest = 0, streak = 0;
      DateOnly? prev = null;

      foreach (var date in sorted)
      {
          if(prev is null || prev.Value.AddDays(-1) == date)
            streak++;
          else
           streak = 1;

          prev = date;
          if(streak > longest) longest = streak;
      }

      var current = sorted[0] >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) ? streak : 0;

      var completionRate = sorted.Count / (double)30 * 100;

      return new StreakResult(current, longest, sorted.Count, Math.Min(completionRate, 100));
  }
}