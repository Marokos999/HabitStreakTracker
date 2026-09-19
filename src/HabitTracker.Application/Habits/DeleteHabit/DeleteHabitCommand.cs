namespace HabitTracker.Application.Habits.DeleteHabit;

public record DeleteHabitCommand(string UserId, Guid HabitId);