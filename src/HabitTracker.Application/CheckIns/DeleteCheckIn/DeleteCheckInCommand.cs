namespace HabitTracker.Application.CheckIns.DeleteCheckIn;

public record DeleteCheckInCommand(string UserId, Guid HabitId, DateOnly Date);