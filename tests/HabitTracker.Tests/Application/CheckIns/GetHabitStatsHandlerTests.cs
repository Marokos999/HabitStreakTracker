using HabitTracker.Application.CheckIns.GetHabitStats;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.CheckIns;

public class GetHabitStatsHandlerTests
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private static Mock<IHabitRepository> HabitRepoWith(Guid habitId, int targetDaysPerWeek = 7, bool archived = false)
    {
        var habitRepo = new Mock<IHabitRepository>();
        habitRepo.Setup(r => r.GetByIdAsync("user1", habitId)).ReturnsAsync(new Habit
        {
            Id = habitId,
            UserId = "user1",
            Name = "Exercise",
            TargetDaysPerWeek = targetDaysPerWeek,
            IsArchived = archived
        });
        return habitRepo;
    }

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

        var handler = new GetHabitStatsHandler(repo.Object, HabitRepoWith(habitId).Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", habitId));

        Assert.Equal(3, result.Streak.CurrentStreak);
        Assert.Equal(3, result.Streak.LongestStreak);
        Assert.Equal(3, result.Streak.TotalCheckIns);
    }

    [Fact]
    public async Task Handle_NoCheckIns_ReturnsZeroStreak()
    {
        var habitId = Guid.NewGuid();
        var repo = new Mock<ICheckInRepository>();
        repo.Setup(r => r.GetByHabitAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync([]);

        var handler = new GetHabitStatsHandler(repo.Object, HabitRepoWith(habitId).Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", habitId));

        Assert.Equal(0, result.Streak.CurrentStreak);
        Assert.Equal(0, result.Streak.TotalCheckIns);
        Assert.Empty(result.RecentCheckInDates);
    }

    [Fact]
    public async Task Handle_RecentCheckInDates_OnlyContainsLastWindowSortedAscending()
    {
        var habitId = Guid.NewGuid();
        var oldest = Today.AddDays(-(GetHabitStatsHandler.RecentDays - 1));
        var repo = new Mock<ICheckInRepository>();
        repo.Setup(r => r.GetByHabitAsync("user1", habitId)).ReturnsAsync(
        [
            new CheckIn { HabitId = habitId, UserId = "user1", Date = Today },
            new CheckIn { HabitId = habitId, UserId = "user1", Date = oldest },
            new CheckIn { HabitId = habitId, UserId = "user1", Date = oldest.AddDays(-1) },
            new CheckIn { HabitId = habitId, UserId = "user1", Date = Today.AddDays(-10) }
        ]);

        var handler = new GetHabitStatsHandler(repo.Object, HabitRepoWith(habitId).Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", habitId));

        Assert.Equal([oldest, Today.AddDays(-10), Today], result.RecentCheckInDates);
        Assert.Equal(4, result.Streak.TotalCheckIns); // streak stats still cover the full history
    }

    [Fact]
    public async Task Handle_UsesHabitsWeeklyTargetForCompletionRate()
    {
        var habitId = Guid.NewGuid();
        var repo = new Mock<ICheckInRepository>();
        // 3x per week over 30 days is about 12.9 check-ins, so 13 check-ins means 100%
        repo.Setup(r => r.GetByHabitAsync("user1", habitId)).ReturnsAsync(
            Enumerable.Range(0, 13)
                .Select(i => new CheckIn { HabitId = habitId, UserId = "user1", Date = Today.AddDays(-2 * i) })
                .ToList());

        var handler = new GetHabitStatsHandler(repo.Object, HabitRepoWith(habitId, targetDaysPerWeek: 3).Object);
        var result = await handler.Handle(new GetHabitStatsQuery("user1", habitId));

        Assert.Equal(100, result.Streak.CompletionRate);
    }

    [Fact]
    public async Task Handle_HabitMissingOrArchived_ThrowsNotFound()
    {
        var habitId = Guid.NewGuid();
        var repo = new Mock<ICheckInRepository>();
        var missing = new Mock<IHabitRepository>();
        missing.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((Habit?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new GetHabitStatsHandler(repo.Object, missing.Object).Handle(new GetHabitStatsQuery("user1", habitId)));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            new GetHabitStatsHandler(repo.Object, HabitRepoWith(habitId, archived: true).Object)
                .Handle(new GetHabitStatsQuery("user1", habitId)));
        repo.Verify(r => r.GetByHabitAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }
}
