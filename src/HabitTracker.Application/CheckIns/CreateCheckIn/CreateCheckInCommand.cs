namespace HabitTracker.Application.CheckIns.CreateCheckIn;

public record CreateCheckInCommand(string UserId, Guid HabitId, DateOnly Date, string Note);