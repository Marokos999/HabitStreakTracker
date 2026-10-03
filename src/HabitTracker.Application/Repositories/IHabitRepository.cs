using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.Repositories;

public interface IHabitRepository
{
    Task<List<Habit>> GetAllAsync(string userId);
    Task<Habit?> GetByIdAsync(string userId, Guid habitId);
    Task CreateAsync(Habit habit);
    Task UpdateAsync(Habit habit);
    Task SoftDeleteAsync(string userId, Guid habitId);
}
