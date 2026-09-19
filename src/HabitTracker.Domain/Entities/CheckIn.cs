namespace HabitTracker.Domain.Entities;

public class CheckIn
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid HabitId { get; set; }
  public string UserId { get; set; } = default!;
  public DateOnly Date { get; set; }
  public string? Note { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}