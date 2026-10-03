using HabitTracker.Domain.Entities;

namespace HabitTracker.Application.Repositories;

public interface ICheckInRepository
{
    Task<List<CheckIn>> GetByDateRangeAsync(string userId, DateOnly from, DateOnly to);
    Task<List<CheckIn>> GetByHabitAsync(string userId, Guid habitId);
    Task CreateAsync(CheckIn checkIn);
    Task DeleteAsync(string userId, Guid habitId, DateOnly date);
}
