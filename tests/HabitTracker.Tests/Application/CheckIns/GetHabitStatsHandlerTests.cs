using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.CheckIns;

public class GetHabitStatsHandlerTests
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task Handle_ReturnsStreakFromCheckIns()
    {
        var habitId = Guid.NewGuid();
        var repo = new Mock<ICheckInRepository>();
        repo.Setup(r => r.GetByHabitAsync("user1", habitId)).ReturnsAsync(
        [
            new CheckIn { HabitId = habitId, UserId = "user1", Date = Today },
            new CheckIn { HabitId = habitId, UserId = "user1", Date = Today.AddDays(-1) },
            new CheckIn { HabitId = habitId, UserId = "user1", Date = Today.AddDays(-2) }
        ]);

        var handler = new GetHabitStatsHandler(repo.Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", habitId));

        Assert.Equal(3, result.Streak.CurrentStreak);
        Assert.Equal(3, result.Streak.LongestStreak);
        Assert.Equal(3, result.Streak.TotalCheckIns);
    }

    [Fact]
    public async Task Handle_NoCheckIns_ReturnsZeroStreak()
    {
        var repo = new Mock<ICheckInRepository>();
        repo.Setup(r => r.GetByHabitAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync([]);

        var handler = new GetHabitStatsHandler(repo.Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", Guid.NewGuid()));

        Assert.Equal(0, result.Streak.CurrentStreak);
        Assert.Equal(0, result.Streak.TotalCheckIns);
    }
}
