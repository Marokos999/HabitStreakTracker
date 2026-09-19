
using HabitTracker.Domain.Services;
using HabitTracker.Domain.ValueObjects;

namespace HabitTracker.Tests.Domain.StreakCalculatorTests;

public class StreakCalculatorTests
{
  [Fact]
  public void Calculate_EmptyList_ReturnsZeroStreak()
  {
    var result =  StreakCalculator.Calculate([]);

    Assert.Equal(0, result.CurrentStreak);
    Assert.Equal(0, result.LongestStreak);
    Assert.Equal(0, result.TotalCheckIns);
  }

  [Fact]
  public void Calculate_ConsecutiveDays_ReturnsCorrectStreak()
  {
     var today = DateOnly.FromDateTime(DateTime.UtcNow);
     var dates = new[] { today, today.AddDays(-1), today.AddDays(-2) };

     var result = StreakCalculator.Calculate(dates);

     Assert.Equal(3, result.CurrentStreak);
     Assert.Equal(3, result.LongestStreak);
     Assert.Equal(3, result.TotalCheckIns);
  }

  [Fact]
public void Calculate_BrokenStreak_CurrentStreakIsZero()
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var dates = new[] { today.AddDays(-5), today.AddDays(-6) };

    var result = StreakCalculator.Calculate(dates);

    Assert.Equal(0, result.CurrentStreak);
    Assert.Equal(2, result.LongestStreak);
}
 [Fact]
    public void Calculate_SingleCheckIn_Today_ReturnsStreakOne()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = StreakCalculator.Calculate([today]);

        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(1, result.LongestStreak);
    }
}