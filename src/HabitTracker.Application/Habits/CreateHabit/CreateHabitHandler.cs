using HabitTracker.Application.Repositories;
using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.Habits.CreateHabit;

public class CreateHabitHandler(IHabitRepository repo)
{
  public async Task<CreateHabitResult> Handle(CreateHabitCommand command)
  {
     var habit = new Habit
     {
       UserId = command.UserId,
       Name = command.Name,
       Description = command.Description,
       Frequency = command.Frequency,
       Color = command.Color,
       TargetDaysPerWeek = command.TargetDaysPerWeek
     };

     await repo.CreateAsync(habit);
     return new CreateHabitResult(habit);
  }
}