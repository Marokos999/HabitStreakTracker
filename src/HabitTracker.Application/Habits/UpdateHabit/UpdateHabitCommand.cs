using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.UpdateHabit;

public record UpdateHabitCommand(
       string UserId,
       Guid HabitId,
       string Name,
       string? Description,
       HabitFrequency Frequency,
       string Color,
       int TargetDaysPerWeek
);