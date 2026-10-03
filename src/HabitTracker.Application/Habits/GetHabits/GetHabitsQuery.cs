namespace HabitTracker.Application.Habits.GetHabits;

// Date is the day used for "checked in today"; defaults to the current UTC date.
public record GetHabitsQuery(string UserId, DateOnly? Date = null);
