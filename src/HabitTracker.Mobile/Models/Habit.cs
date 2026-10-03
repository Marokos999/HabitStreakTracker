namespace HabitTracker.Mobile.Models;

public class Habit
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Frequency { get; set; }
    public string Color { get; set; } = "#6366F1";
    public int TargetDaysPerWeek { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}
