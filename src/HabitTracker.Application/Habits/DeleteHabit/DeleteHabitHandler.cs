using HabitTracker.Application.Repositories;

namespace HabitTracker.Application.Habits.DeleteHabit;

public class DeleteHabitHandler(IHabitRepository repo)
{
  public async Task Handle(DeleteHabitCommand command)
  {
    await repo.SoftDeleteAsync(command.UserId, command.HabitId);
  }
}