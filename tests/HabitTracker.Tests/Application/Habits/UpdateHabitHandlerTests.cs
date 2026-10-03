using HabitTracker.Application.Habits.UpdateHabit;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using HabitTracker.Domain.Enums;
using Moq;

namespace HabitTracker.Tests.Application.Habits;

public class UpdateHabitHandlerTests
{
    [Fact]
    public async Task Handle_UpdateHavits()
    {
        // Given
        var habitId = Guid.NewGuid();
        var existing = new Habit
        {
            Id = habitId,
            UserId = "user1",
            Name = "Old Name"
        };
        var mockRepo = new Mock<IHabitRepository>();
        mockRepo.Setup(s => s.GetByIdAsync("user1", habitId)).ReturnsAsync(existing);

        var command = new UpdateHabitCommand
        (
          "user1", habitId, "New Name", null, HabitFrequency.Daily, "#6366F1", 7
        );

        var handler = new UpdateHabitHandler(mockRepo.Object);
        await handler.Handle(command);

        Assert.Equal("New Name", existing.Name);
        mockRepo.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact]
    public async Task Handle_HabitNotFound_ThrowsNetNotFoundException()
    {
        var mockRepo = new Mock<IHabitRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((Habit?)null);

        var handler = new UpdateHabitHandler(mockRepo.Object);
        var command = new UpdateHabitCommand
        (
          "user1", Guid.NewGuid(), "Name", null, HabitFrequency.Daily, "#6366F1", 7
        );

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command));
    }
}
