using HabitTracker.Application.Repositories;

namespace HabitTracker.Application.Habits.GetHabits;

public class GetHabitsHandler(IHabitRepository repo, ICheckInRepository checkInRepo)
{
    public async Task<GetHabitsResult> Handle(GetHabitsQuery query)
    {
        var habits = await repo.GetAllAsync(query.UserId);

        // The client passes its local date, so "today" matches what the user sees on the device.
        var date = query.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var todaysCheckIns = await checkInRepo.GetByDateRangeAsync(query.UserId, date, date);
        var checkedIn = todaysCheckIns.Select(c => c.HabitId).ToHashSet();

        return new GetHabitsResult(habits, checkedIn);
    }
}
