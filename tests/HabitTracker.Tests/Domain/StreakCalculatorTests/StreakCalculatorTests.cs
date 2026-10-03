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
    public void Calculate_DailyHabitCheckedEveryDayOf30_CompletionIs100()
    {
        var dates = Enumerable.Range(0, 30).Select(i => Today.AddDays(-i));

        var result = StreakCalculator.Calculate(dates, Today);

        Assert.Equal(100, result.CompletionRate);
    }

    [Fact]
    public void Calculate_CompletionRate_OnlyCountsLast30Days()
    {
        var dates = new[] { Today, Today.AddDays(-29), Today.AddDays(-30), Today.AddDays(-90) };

        var result = StreakCalculator.Calculate(dates, Today);

        Assert.Equal(4, result.TotalCheckIns);
        Assert.Equal(2 / 30.0 * 100, result.CompletionRate, precision: 5);
    }

    [Fact]
    public void Calculate_WeeklyTarget_ScalesExpectedCheckIns()
    {
        // 3 days per week over 30 days expects 3 * 30 / 7 check-ins, so 6 check-ins is about 46.7%
        var dates = Enumerable.Range(0, 6).Select(i => Today.AddDays(-i * 4));

        var result = StreakCalculator.Calculate(dates, Today, targetDaysPerWeek: 3);

        Assert.Equal(6 / (3 * 30 / 7.0) * 100, result.CompletionRate, precision: 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(99)]
    public void Calculate_OutOfRangeTarget_IsClampedToValidRange(int target)
    {
        var dates = new[] { Today };

        var result = StreakCalculator.Calculate(dates, Today, target);

        Assert.InRange(result.CompletionRate, 0, 100);
    }

    [Fact]
    public void Calculate_UnsortedInput_ReturnsCorrectStreak()
    {
        var dates = new[] { Today.AddDays(-2), Today, Today.AddDays(-1) };

        var result = StreakCalculator.Calculate(dates, Today);

        Assert.Equal(3, result.CurrentStreak);
    }
}
