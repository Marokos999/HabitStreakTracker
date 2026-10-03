using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.CheckIns.CreateCheckIn;

public class CreateCheckInHandler(ICheckInRepository repo, IHabitRepository habitRepo)
{
  public async Task<CreateCheckInResult> Handle(CreateCheckInCommand command)
  {
    var habit = await habitRepo.GetByIdAsync(command.UserId, command.HabitId);
    if (habit is null || habit.IsArchived)
      throw new KeyNotFoundException($"Habit {command.HabitId} not found.");

    var checkIn = new CheckIn()
    {
      HabitId = command.HabitId,
      UserId = command.UserId,
      Date = command.Date,
      Note = command.Note
    };

    await repo.CreateAsync(checkIn);
    return new CreateCheckInResult(checkIn);
  }
}
