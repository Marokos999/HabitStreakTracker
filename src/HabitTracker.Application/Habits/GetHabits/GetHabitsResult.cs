using HabitTracker.Domain.Entities;
namespace HabitTracker.Application.Habits.GetHabits;

public record GetHabitsResult(List<Habit> Habits);
