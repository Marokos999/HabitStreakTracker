using HabitTracker.Domain.Services;

namespace HabitTracker.Tests.Domain.StreakCalculatorTests;

public class StreakCalculatorTests
{
  private static readonly DateOnly Today = new(2026, 10, 3);

  [Fact]
  public void Calculate_EmptyList_ReturnsZeroStreak()
  {
    var result = StreakCalculator.Calculate([], Today);

    Assert.Equal(0, result.CurrentStreak);
    Assert.Equal(0, result.LongestStreak);
    Assert.Equal(0, result.TotalCheckIns);
  }

  [Fact]
  public void Calculate_ConsecutiveDays_ReturnsCorrectStreak()
  {
    var dates = new[] { Today, Today.AddDays(-1), Today.AddDays(-2) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(3, result.CurrentStreak);
    Assert.Equal(3, result.LongestStreak);
    Assert.Equal(3, result.TotalCheckIns);
  }

  [Fact]
  public void Calculate_BrokenStreak_CurrentStreakIsZero()
  {
    var dates = new[] { Today.AddDays(-5), Today.AddDays(-6) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(0, result.CurrentStreak);
    Assert.Equal(2, result.LongestStreak);
  }

  [Fact]
  public void Calculate_SingleCheckIn_Today_ReturnsStreakOne()
  {
    var result = StreakCalculator.Calculate([Today], Today);

    Assert.Equal(1, result.CurrentStreak);
    Assert.Equal(1, result.LongestStreak);
  }

  [Fact]
  public void Calculate_RecentRunAndOlderRun_CurrentIsRecentRun()
  {
    var dates = new[] { Today, Today.AddDays(-1), Today.AddDays(-5), Today.AddDays(-6), Today.AddDays(-7) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(2, result.CurrentStreak);
    Assert.Equal(3, result.LongestStreak);
  }

  [Fact]
  public void Calculate_DuplicateDates_AreCountedOnce()
  {
    var dates = new[] { Today, Today, Today.AddDays(-1) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(2, result.CurrentStreak);
    Assert.Equal(2, result.TotalCheckIns);
  }

  [Fact]
  public void Calculate_LastCheckInYesterday_StreakStillActive()
  {
    var dates = new[] { Today.AddDays(-1), Today.AddDays(-2) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(2, result.CurrentStreak);
  }

  [Fact]
  public void Calculate_UnsortedInput_ReturnsCorrectStreak()
  {
    var dates = new[] { Today.AddDays(-2), Today, Today.AddDays(-1) };

    var result = StreakCalculator.Calculate(dates, Today);

    Assert.Equal(3, result.CurrentStreak);
  }
}
