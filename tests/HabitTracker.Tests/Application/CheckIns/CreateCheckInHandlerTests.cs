using HabitTracker.Application.CheckIns.CreateCheckIn;
using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;
using Moq;

namespace HabitTracker.Tests.Application.CheckIns;

public class CreateCheckInHandlerTests
{
  private static readonly DateOnly Today = new(2026, 10, 3);

  [Fact]
  public async Task Handle_OwnedHabit_CreatesCheckIn()
  {
    var habitId = Guid.NewGuid();
    var checkInRepo = new Mock<ICheckInRepository>();
    var habitRepo = new Mock<IHabitRepository>();
    habitRepo.Setup(r => r.GetByIdAsync("user1", habitId))
      .ReturnsAsync(new Habit { Id = habitId, UserId = "user1", Name = "Exercise" });

    var handler = new CreateCheckInHandler(checkInRepo.Object, habitRepo.Object);
    var result = await handler.Handle(new CreateCheckInCommand("user1", habitId, Today, "done"));

    Assert.Equal(habitId, result.CheckIn.HabitId);
    Assert.Equal("user1", result.CheckIn.UserId);
    checkInRepo.Verify(r => r.CreateAsync(result.CheckIn), Times.Once);
  }

  [Fact]
  public async Task Handle_HabitNotOwnedOrMissing_ThrowsNotFoundAndDoesNotWrite()
  {
    var checkInRepo = new Mock<ICheckInRepository>();
    var habitRepo = new Mock<IHabitRepository>();
    habitRepo.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((Habit?)null);

    var handler = new CreateCheckInHandler(checkInRepo.Object, habitRepo.Object);

    await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      handler.Handle(new CreateCheckInCommand("user2", Guid.NewGuid(), Today, null)));
    checkInRepo.Verify(r => r.CreateAsync(It.IsAny<CheckIn>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ArchivedHabit_ThrowsNotFound()
  {
    var habitId = Guid.NewGuid();
    var checkInRepo = new Mock<ICheckInRepository>();
    var habitRepo = new Mock<IHabitRepository>();
    habitRepo.Setup(r => r.GetByIdAsync("user1", habitId))
      .ReturnsAsync(new Habit { Id = habitId, UserId = "user1", Name = "Old", IsArchived = true });

    var handler = new CreateCheckInHandler(checkInRepo.Object, habitRepo.Object);

    await Assert.ThrowsAsync<KeyNotFoundException>(() =>
      handler.Handle(new CreateCheckInCommand("user1", habitId, Today, null)));
  }
}
