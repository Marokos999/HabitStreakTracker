using HabitTracker.Application.CheckIns.DeleteCheckIn;
using HabitTracker.Application.Repositories;
using Moq;

namespace HabitTracker.Tests.Application.CheckIns;

public class DeleteCheckInHandlerTests
{
    [Fact]
    public async Task Handle_DeletesCheckInForUserHabitAndDate()
    {
        var habitId = Guid.NewGuid();
        var date = new DateOnly(2026, 10, 3);
        var repo = new Mock<ICheckInRepository>();

        var handler = new DeleteCheckInHandler(repo.Object);
        await handler.Handle(new DeleteCheckInCommand("user1", habitId, date));

        repo.Verify(r => r.DeleteAsync("user1", habitId, date), Times.Once);
    }
}
