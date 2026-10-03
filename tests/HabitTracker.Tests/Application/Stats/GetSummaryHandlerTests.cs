using HabitTracker.Application.Repositories;
using HabitTracker.Application.Stats.GetSummary;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.Stats;

public class GetSummaryHandlerTests
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task Handle_NoHabits_ReturnsZeros()
    {
        var habitRepo = new Mock<IHabitRepository>();
        habitRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([]);
        var checkInRepo = new Mock<ICheckInRepository>();

        var handler = new GetSummaryHandler(habitRepo.Object, checkInRepo.Object);
        var result = await handler.Handle(new GetSummaryQuery("user1"));

        Assert.Equal(new GetSummaryResult(0, 0, 0, 0), result);
        checkInRepo.Verify(r => r.GetByHabitAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleHabits_AggregatesStats()
    {
        var active = new Habit { Id = Guid.NewGuid(), UserId = "user1", Name = "Exercise" };
        var idle = new Habit { Id = Guid.NewGuid(), UserId = "user1", Name = "Read" };
        var habitRepo = new Mock<IHabitRepository>();
        habitRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([active, idle]);

        var checkInRepo = new Mock<ICheckInRepository>();
        checkInRepo.Setup(r => r.GetByHabitAsync("user1", active.Id)).ReturnsAsync(
        [
            new CheckIn { HabitId = active.Id, UserId = "user1", Date = Today },
            new CheckIn { HabitId = active.Id, UserId = "user1", Date = Today.AddDays(-1) }
        ]);
        checkInRepo.Setup(r => r.GetByHabitAsync("user1", idle.Id)).ReturnsAsync([]);

        var handler = new GetSummaryHandler(habitRepo.Object, checkInRepo.Object);
        var result = await handler.Handle(new GetSummaryQuery("user1"));

        Assert.Equal(2, result.TotalHabits);
        Assert.Equal(1, result.CheckedInToday);
        Assert.Equal(2, result.BestCurrentStreak);
        // active habit: 2 / 30 * 100, idle habit: 0 -> average of both
        Assert.Equal(2 / 30.0 * 100 / 2, result.AverageCompletionRate, precision: 5);
    }

    [Fact]
    public async Task Handle_CheckInOnlyYesterday_NotCountedAsCheckedInToday()
    {
        var habit = new Habit { Id = Guid.NewGuid(), UserId = "user1", Name = "Exercise" };
        var habitRepo = new Mock<IHabitRepository>();
        habitRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([habit]);
        var checkInRepo = new Mock<ICheckInRepository>();
        checkInRepo.Setup(r => r.GetByHabitAsync("user1", habit.Id)).ReturnsAsync(
        [
            new CheckIn { HabitId = habit.Id, UserId = "user1", Date = Today.AddDays(-1) }
        ]);

        var handler = new GetSummaryHandler(habitRepo.Object, checkInRepo.Object);
        var result = await handler.Handle(new GetSummaryQuery("user1"));

        Assert.Equal(0, result.CheckedInToday);
        Assert.Equal(1, result.BestCurrentStreak);
    }
}
