using HabitTracker.Domain.Enums;

namespace HabitTracker.Application.Habits.CreateHabit;

public record CreateHabitCommand(
       string UserId,
       string Name,
       string? Description,
       HabitFrequency Frequency,
       string Color,
       int TargetDaysPerWeek);