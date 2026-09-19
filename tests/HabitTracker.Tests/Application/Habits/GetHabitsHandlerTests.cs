using HabitTracker.Application.Habits.GetHabits;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.Habits;

public class GetHabitsHandlerTests
{
  [Fact]
  public async Task Handle_ReturnsHabitsForUser()
  {
    var mockRepo = new Mock<IHabitRepository>();
    var habits = new List<Habit>
    {
      new Habit {UserId = "user1", Name = "Exercise"},
      new Habit {UserId = "user1", Name = "Read"}
    };

    mockRepo.Setup(s => s.GetAllAsync("user1")).ReturnsAsync(habits);

    var handler = new GetHabitsHandler(mockRepo.Object);
    var results = await handler.Handle(new GetHabitsQuery("user1"));

    Assert.Equal(2, results.Habits.Count);
    mockRepo.Verify(r => r.GetAllAsync("user1"), Times.Once);
  }

  [Fact]
  public async Task Handle_emptyList_ReturnsEmpty()
  {
    var mockRepo = new Mock<IHabitRepository>();
    mockRepo.Setup(r => r.GetAllAsync("user1")).ReturnsAsync([]);

    var handler = new GetHabitsHandler(mockRepo.Object);
    var result = await handler.Handle(new GetHabitsQuery("user1"));

    Assert.Empty(result.Habits);
  }
}