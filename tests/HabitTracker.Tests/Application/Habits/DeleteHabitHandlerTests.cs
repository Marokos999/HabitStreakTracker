using HabitTracker.Application.Habits.DeleteHabit;
using HabitTracker.Application.Repositories;
using Moq;

namespace HabitTracker.Tests.Application.Habits;

public class DeleteHabitHandlerTests
{
    [Fact]
    public async Task Habdle_CallsSoftDelete()
    {
        var habitId = Guid.NewGuid();
        var mockRepo = new Mock<IHabitRepository>();

        var handler = new DeleteHabitHandler(mockRepo.Object);
        await handler.Handle(new DeleteHabitCommand("user1", habitId));

        mockRepo.Verify(s => s.SoftDeleteAsync("user1", habitId), Times.Once);
    }
}
