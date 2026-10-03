using HabitTracker.Application.Repositories;

namespace HabitTracker.Application.Habits.UpdateHabit;

public class UpdateHabitHandler(IHabitRepository repo)
{
    public async Task Handle(UpdateHabitCommand command)
    {
        HabitValidator.Validate(command.Name, command.Description, command.Frequency, command.Color,
                                command.TargetDaysPerWeek);

        var habit = await repo.GetByIdAsync(command.UserId, command.HabitId)
                    ?? throw new KeyNotFoundException($"Habit {command.HabitId} not found.");

        habit.Name = command.Name.Trim();
        habit.Description = command.Description;
        habit.Color = command.Color;
        habit.Frequency = command.Frequency;
        habit.TargetDaysPerWeek = command.TargetDaysPerWeek;

        await repo.UpdateAsync(habit);
    }
}
