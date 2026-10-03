using HabitTracker.Application.Habits.CreateHabit;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Enums;
using Moq;

namespace HabitTracker.Tests.Application.Habits;

public class CreateHabitHandlerTests
{
    [Fact]
    public async Task Handle_CreatesHabitsAndReturnsIt()
    {
        var mockRepo = new Mock<IHabitRepository>();
        var command = new CreateHabitCommand(
          "user1",
          "Exercise",
           null,
           HabitFrequency.Daily,
           "#6366F1",
           7);

        var handler = new CreateHabitHandler(mockRepo.Object);
        var result = await handler.Handle(command);

        Assert.Equal("Exercise", result.Habit.Name);
        Assert.Equal("user1", result.Habit.UserId);
        mockRepo.Verify(r => r.CreateAsync(result.Habit), Times.Once);
    }
}
