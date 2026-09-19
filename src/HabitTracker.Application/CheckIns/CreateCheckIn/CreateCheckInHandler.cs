using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.CheckIns.CreateCheckIn;

public class CreateCheckInHandler(ICheckInRepository repo)
{
  public async Task<CreateCheckInResult> Handle(CreateCheckInCommand command)
  {
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