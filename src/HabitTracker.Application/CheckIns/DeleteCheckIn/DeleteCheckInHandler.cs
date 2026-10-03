using HabitTracker.Application.Repositories;

namespace HabitTracker.Application.CheckIns.DeleteCheckIn;

public class DeleteCheckInHandler(ICheckInRepository repo)
{
    public async Task Handle(DeleteCheckInCommand command)
    {
        await repo.DeleteAsync(command.UserId, command.HabitId, command.Date);
    }
}
