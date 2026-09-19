using HabitTracker.Domain.Enums;

namespace HabitTracker.Domain.Entities;

public class Habit
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string UserId { get; set; } = default!;
  public string Name { get; set; } = default!;
  public string? Description { get; set; } 
  public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
  public string Color { get; set; } = "#6366F1";
  public int TargetDaysPerWeek { get; set; } = 7;
  public bool IsArchived { get; set; } = false;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;





}