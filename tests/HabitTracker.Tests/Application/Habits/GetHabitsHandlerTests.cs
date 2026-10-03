using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.Habits;

public class GetHabitsHandlerTests
{
    private static readonly DateOnly Day = new(2026, 10, 3);

    [Fact]
    public async Task Handle_ReturnsHabitsForUser()
    {
        var mockRepo = new Mock<IHabitRepository>();
        var checkIns = new Mock<ICheckInRepository>();
        var habits = new List<Habit>
        {
            new() { UserId = "user1", Name = "Exercise" },
            new() { UserId = "user1", Name = "Read" }
        };
        mockRepo.Setup(s => s.GetAllAsync("user1")).ReturnsAsync(habits);
        checkIns.Setup(r => r.GetByDateRangeAsync("user1", Day, Day)).ReturnsAsync([]);

        var handler = new GetHabitsHandler(mockRepo.Object, checkIns.Object);
        var results = await handler.Handle(new GetHabitsQuery("user1", Day));

        Assert.Equal(2, results.Habits.Count);
        Assert.Empty(results.CheckedInHabitIds);
        mockRepo.Verify(r => r.GetAllAsync("user1"), Times.Once);
    }

    [Fact]
    public async Task Handle_emptyList_ReturnsEmpty()
    {
        var mockRepo = new Mock<IHabitRepository>();
        var checkIns = new Mock<ICheckInRepository>();
        mockRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([]);
        checkIns.Setup(r => r.GetByDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync([]);

        var handler = new GetHabitsHandler(mockRepo.Object, checkIns.Object);
        var result = await handler.Handle(new GetHabitsQuery("user1"));

        Assert.Empty(result.Habits);
    }

    [Fact]
    public async Task Handle_MarksHabitsCheckedInOnRequestedDate()
    {
        var done = new Habit { UserId = "user1", Name = "Exercise" };
        var pending = new Habit { UserId = "user1", Name = "Read" };
        var mockRepo = new Mock<IHabitRepository>();
        var checkIns = new Mock<ICheckInRepository>();
        mockRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([done, pending]);
        checkIns.Setup(r => r.GetByDateRangeAsync("user1", Day, Day)).ReturnsAsync(
        [
            new CheckIn { UserId = "user1", HabitId = done.Id, Date = Day }
        ]);

        var handler = new GetHabitsHandler(mockRepo.Object, checkIns.Object);
        var result = await handler.Handle(new GetHabitsQuery("user1", Day));

        Assert.Contains(done.Id, result.CheckedInHabitIds);
        Assert.DoesNotContain(pending.Id, result.CheckedInHabitIds);
    }

    [Fact]
    public async Task Handle_NoDate_UsesCurrentUtcDate()
    {
        var mockRepo = new Mock<IHabitRepository>();
        var checkIns = new Mock<ICheckInRepository>();
        mockRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([]);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        checkIns.Setup(r => r.GetByDateRangeAsync("user1", today, today)).ReturnsAsync([]);

        var handler = new GetHabitsHandler(mockRepo.Object, checkIns.Object);
        await handler.Handle(new GetHabitsQuery("user1"));

        checkIns.Verify(r => r.GetByDateRangeAsync("user1", today, today), Times.Once);
    }
}
