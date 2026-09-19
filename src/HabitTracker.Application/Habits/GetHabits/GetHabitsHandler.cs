using HabitTracker.Application.Repositories;

namespace HabitTracker.Application.Habits.GetHabits;

public class GetHabitsHandler(IHabitRepository repo)
{
  public async Task<GetHabitsResult> Handle(GetHabitsQuery query)
  {
    var habits = await repo.GetAllAsync(query.UserId);
    return new GetHabitsResult(habits);
  }
}